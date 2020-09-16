import { BehaviorSubject, Subject, Observable } from 'rxjs';

import { GangContext } from '../context';
import {
  GangWebSocket,
  GangConnectionState,
  GangCommandWrapper,
  GangUrlBuilder,
  NO_RETRY,
  RETRY_INIT,
  RETRY_MAX,
  GangWebSocketFactory,
} from '../models';

export class GangService {
  private readonly rootUrl: string;
  private retry = RETRY_INIT;
  private retrying: number;

  retryingIn: number = undefined;
  private webSocket: GangWebSocket;
  private connectionSubject: BehaviorSubject<GangConnectionState>;

  get connectionState(): GangConnectionState {
    return this.connectionSubject.value;
  }
  get isConnected(): boolean {
    return this.connectionSubject.value === GangConnectionState.connected;
  }
  memberId: string;
  isHost: boolean;

  private memberConnectedSubject: Subject<string>;
  private memberDisconnectedSubject: Subject<string>;
  private commandSubject: Subject<GangCommandWrapper<unknown>>;
  private stateSubject: BehaviorSubject<object>;
  private unsentCommands: GangCommandWrapper<unknown>[] = [];

  onConnection: Observable<GangConnectionState>;
  onMemberConnected: Observable<string>;
  onMemberDisconnected: Observable<string>;
  onCommand: Observable<GangCommandWrapper<unknown>>;
  onState: Observable<unknown>;

  constructor(private webSocketFactory: GangWebSocketFactory) {
    if (!location) throw new Error('required location object not found');

    const protocol = location.protocol.replace('http', 'ws');
    const host = location.host;
    this.rootUrl = `${protocol}//${host}/`;

    this.onConnection = this.connectionSubject = new BehaviorSubject(
      GangConnectionState.disconnected
    );

    this.onMemberConnected = this.memberConnectedSubject = new Subject<
      string
    >();
    this.onMemberDisconnected = this.memberDisconnectedSubject = new Subject<
      string
    >();
    this.onCommand = this.commandSubject = new Subject<
      GangCommandWrapper<unknown>
    >();
    this.onState = this.stateSubject = new BehaviorSubject<object>(undefined);
  }

  async connect(url: string, gangId: string, token?: string): Promise<void> {
    if (this.isConnected) await this.disconnect('reconnect');

    return new Promise<void>((resolve, reject) => {
      this.connectionSubject.next(GangConnectionState.connecting);
      const connectUrl = GangUrlBuilder.from(this.rootUrl + url)
        .set('gangId', gangId)
        .set('token', token)
        .build();
      GangContext.logger('GangService.connect', this.rootUrl + url, connectUrl);

      this.webSocket = this.webSocketFactory(
        connectUrl,
        (e: Event) => {
          GangContext.logger('GangService.onopen', e);

          this.connectionSubject.next(GangConnectionState.connected);
          this.retry = RETRY_INIT;
          this.retryingIn = undefined;
          resolve();

          let wrapper: GangCommandWrapper<unknown>;
          while ((wrapper = this.unsentCommands.shift()))
            this.sendCommandWrapper(wrapper);

          clearRetryConnect();
        },
        (e: Event) => {
          GangContext.logger('GangService.onerror', e);

          this.connectionSubject.next(GangConnectionState.error);
          reject(e);
        },
        (e: CloseEvent) => {
          GangContext.logger('GangService.onclose', e);

          this.connectionSubject.next(GangConnectionState.disconnected);
          this.memberDisconnectedSubject.next(this.memberId);

          if (!e.reason) retryConnect();
        }
      );

      this.webSocket.subscribe((e: MessageEvent) => {
        const reader = new FileReader();

        reader.onload = () => {
          const messageType = reader.result[0];
          const messageData = reader.result.slice(1) as string;

          GangContext.logger(
            'GangService.onmessage type:',
            messageType,
            'data:',
            messageData
          );

          switch (messageType) {
            default:
              throw new Error(`unknown message type: ${messageType}`);
            case 'H':
              this.isHost = true;
              this.memberId = messageData;
              this.memberConnectedSubject.next(messageData);
              break;
            case 'M':
              this.isHost = false;
              this.memberId = messageData;
              this.memberConnectedSubject.next(messageData);
              break;
            case '+':
              this.memberConnectedSubject.next(messageData);
              break;
            case '-':
              this.memberDisconnectedSubject.next(messageData);
              break;
            case 'C':
              this.commandSubject.next(JSON.parse(messageData));
              break;
            case 'S':
              this.stateSubject.next({
                ...this.stateSubject.value,
                ...JSON.parse(messageData)
              });
              break;
          }
        };
        reader.readAsText(e.data);
      });

      const retryConnect = (() => {
        if (
          this.retry === NO_RETRY ||
          this.retrying ||
          this.connectionState === GangConnectionState.connected
        )
          return;

        GangContext.logger('GangService.retryConnect in', this.retry);
        this.retryingIn = this.retry;
        this.retrying = window.setInterval(() => {
          this.retryingIn--;
          if (this.retryingIn === 0) {
            clearRetryConnect();
            this.connect(url, gangId, token);
          }
        }, 1000);

        if (this.retry < RETRY_MAX) this.retry *= 2;
      }).bind(this);

      const clearRetryConnect = (() => {
        if (this.retrying) {
          clearInterval(this.retrying);
          this.retrying = null;
        }
      }).bind(this);

      retryConnect();
    });
  }

  disconnect(reason = 'disconnected'): Promise<void> {
    GangContext.logger('GangService.disconnect');

    return new Promise((resolve) => {
      if (this.webSocket) {
        const wait = this.connectionSubject.subscribe((state) => {
          if (state === GangConnectionState.disconnected) {
            GangContext.logger('GangService.disconnect disconnected');

            wait.unsubscribe();
            resolve();
          }
        });

        this.retry = NO_RETRY;
        this.webSocket.close(reason);
      } else {
        resolve();
      }
    });
  }

  /**
   * Map gang events to the component
   *
   * @param component the component to map to
   */
  mapEvents<TState>(
    component: {
      disconnectedCallback?: () => void;
      onConnection?: (connectionState: GangConnectionState) => void;
      onState?: (state: TState) => void;
      onCommand?: (command: unknown) => void;
      onMemberConnected?: (memberId: string) => void;
      onMemberDisconnected?: (memberId: string) => void;
    }
  ): void {
    const subs: { unsubscribe: () => undefined }[] = [];
    [
      'onConnection',
      'onState',
      'onCommand',
      'onMemberConnected',
      'onMemberDisconnected',
    ].forEach((key) => {
      if (component[key])
        subs.push(this[key].subscribe((e: unknown) => component[key](e)));
    });

    const disconnectedCallback = component.disconnectedCallback;
    component.disconnectedCallback = () => {
      if (disconnectedCallback) disconnectedCallback();
      subs.forEach((sub) => sub.unsubscribe());
    };
  }

  /**
   * Map the actions to a component, injecting this service
   *
   * @param component the component to map to
   * @param actions a map of the executors e.g. { actionOne, actionTwo }
   */
  mapActions<C extends { [K in keyof A]: any }, A>(
    component: C,
    actions: A
  ) {

    Object.keys(actions).forEach(key => {
      component[key] = actions[key](this);
    });
  }

  /**
   * Executes a command locally no data is sent to the host
   *
   * @param type Command type name
   * @param command Command
   */
  executeCommand<T>(type: string, command: T): void {
    const wrapper = new GangCommandWrapper(type, command);
    GangContext.logger('GangService.executeCommand', {
      wrapper,
      isConnected: this.isConnected,
    });

    this.commandSubject.next(wrapper);
  }

  /**
   * Sends a command to the host member
   * await this if you expect a reply command from the host
   *
   * @param type Command type name
   * @param command Command
   *
   * @returns a promise which resolves if a reply command is
   * received from the host having the same sequence number
   * or after 10s (promise is not rejected)
   */
  async sendCommand<T>(type: string, command: T): Promise<void> {
    const wrapper = new GangCommandWrapper(type, command);
    GangContext.logger('GangService.sendCommand', {
      wrapper,
      isConnected: this.isConnected,
    });

    if (!this.isConnected) {
      this.commandSubject.next(wrapper);
      this.unsentCommands.push(wrapper);
      return;
    }

    if (this.isHost) {
      this.commandSubject.next(wrapper);
      return;
    }

    await this.sendCommandWrapper(wrapper);
  }

  private sn = new Uint16Array(1);
  private sendCommandWrapper<T>(wrapper: GangCommandWrapper<T>): Promise<void> {
    const sn = (this.sn[0] += 1);
    this.send([this.sn, JSON.stringify(wrapper)]);

    GangContext.logger('GangService.sendCommandWrapper', wrapper, sn);

    return new Promise((resolve) => {
      const sub = this.onCommand.subscribe((c) => {
        if (c.sn == sn) {
          sub.unsubscribe();
          resolve();
        }
      });

      setTimeout(() => {
        sub.unsubscribe();
        resolve();
      }, 10000);
    });
  }

  sendState(state: object): void {
    if (!this.isHost) throw new Error('only host can send state');

    this.stateSubject.next(state);
    this.send([JSON.stringify(state)]);

    GangContext.logger('GangService.sendState', state);
  }

  private send(parts: BlobPart[]): void {
    const blob = new Blob(parts, {
      type: 'text/plain',
    });
    this.webSocket.send(blob);
  }

  waitForCommand<T>(
    type: string,
    predicate: (c: T) => boolean,
    options?: {
      timeout?: number;
    }
  ): Promise<void> {
    const test: (c: GangCommandWrapper<unknown>) => boolean = (c) => {
      return (
        (!type || type === c.type) && (!predicate || predicate(c.command as T))
      );
    };

    return new Promise((resolve, reject) => {
      const sub = this.onCommand.subscribe((c) => {
        if (test(c)) {
          sub.unsubscribe();
          resolve();
        }
      });

      setTimeout(() => {
        sub.unsubscribe();
        reject();
      }, options?.timeout || 10000);
    });
  }

  waitForState<T>(
    predicate: (s: T) => boolean,
    options?: {
      timeout?: number;
    }
  ): Promise<void> {
    return new Promise((resolve, reject) => {
      const sub = this.onState.subscribe((s) => {
        if (predicate(s as T)) {
          sub.unsubscribe();
          resolve();
        }
      });

      setTimeout(() => {
        sub.unsubscribe();
        reject();
      }, options?.timeout || 10000);
    });
  }
}
