import { BehaviorSubject, Subject, Observable, Subscription } from 'rxjs';

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
  private authenticatedSubject: Subject<string>;
  private memberDisconnectedSubject: Subject<string>;
  private commandSubject: Subject<GangCommandWrapper<unknown>>;
  private stateSubject: BehaviorSubject<Record<string, unknown>>;
  private unsentCommands: GangCommandWrapper<unknown>[] = [];

  onConnection: Observable<GangConnectionState>;
  onAuthenticated: Observable<string>;
  onMemberConnected: Observable<string>;
  onMemberDisconnected: Observable<string>;
  onCommand: Observable<GangCommandWrapper<unknown>>;
  onState: Observable<unknown>;

  constructor(
    private webSocketFactory: GangWebSocketFactory,
    initialState: Record<string, unknown> = undefined) {
    if (!location) throw new Error('required location object not found');

    const protocol = location.protocol.replace('http', 'ws');
    const host = location.host;
    this.rootUrl = `${protocol}//${host}/`;

    this.onConnection = this.connectionSubject = new BehaviorSubject(GangConnectionState.disconnected);
    this.onAuthenticated = this.authenticatedSubject = new Subject<string>();
    this.onMemberConnected = this.memberConnectedSubject = new Subject<string>();
    this.onMemberDisconnected = this.memberDisconnectedSubject = new Subject<string>();
    this.onCommand = this.commandSubject = new Subject<GangCommandWrapper<Record<string, unknown>>>();
    this.onState = this.stateSubject = new BehaviorSubject<Record<string, unknown>>(initialState);
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
        const data = e.data as ArrayBuffer;
        const messageType = readString(data, 0, 1);

        GangContext.logger('GangService.onmessage:', readString(data));

        switch (messageType) {
          default:
            throw new Error(`unknown message type: ${messageType}`);
          case 'H': {
            this.isHost = true;
            this.memberId = readString(data, 1);
            this.memberConnectedSubject.next(this.memberId);
            break;
          }
          case 'M': {
            this.isHost = false;
            this.memberId = readString(data, 1);
            this.memberConnectedSubject.next(this.memberId);
            break;
          }
          case 'A': {
            this.authenticatedSubject.next(readString(data, 1));
            break;
          }
          case '+': {
            const memberId = readString(data, 1);
            this.memberConnectedSubject.next(memberId);
            break;
          }
          case '-': {
            const memberId = readString(data, 1);
            this.memberDisconnectedSubject.next(memberId);
            break;
          }
          case 'C': {
            const commandWrapper =
              data.byteLength > 9 ? JSON.parse(readString(data, 9)) : {};
            commandWrapper.sn = readUint32(data, 1);
            this.commandSubject.next(commandWrapper);
            break;
          }
          case 'S': {
            const state = JSON.parse(readString(data, 1));
            this.stateSubject.next({
              ...this.stateSubject.value,
              ...state,
            });
            break;
          }
        }
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
        let wait: Subscription = null;

        wait = this.connectionSubject.subscribe((state) => {
          if (state === GangConnectionState.disconnected) {
            GangContext.logger('GangService.disconnect disconnected');

            wait?.unsubscribe();
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
  mapEvents<TState>(component: {
    disconnectedCallback?: () => void;
    onGangConnection?: (connectionState: GangConnectionState) => void;
    onGangAuthenticated?: (token: string) => void;
    onGangState?: (state: TState) => void;
    onGangCommand?: (command: unknown) => void;
    onGangMemberConnected?: (memberId: string) => void;
    onGangMemberDisconnected?: (memberId: string) => void;
  }): void {
    const subs: { unsubscribe: () => undefined }[] = [];
    [
      'Connection',
      'Authenticated',
      'State',
      'Command',
      'MemberConnected',
      'MemberDisconnected',
    ].forEach((key) => {
      const componentKey = `onGang${key}`;
      const serviceKey = `on${key}`;

      if (component[componentKey])
        subs.push(this[serviceKey].subscribe((e: unknown) => component[componentKey](e)));

      else if (component[serviceKey] !== undefined)
        console.warn(`${serviceKey} changed to ${componentKey}, please update your code`, component)
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
  mapActions<C extends { [K in keyof A]: unknown }, A>(
    component: C,
    actions: A
  ): void {
    Object.keys(actions).forEach((key) => {
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

  private sn = 0;
  private sendCommandWrapper<T>(wrapper: GangCommandWrapper<T>): Promise<void> {
    const sn = ++this.sn;
    this.send(JSON.stringify(wrapper), sn);

    GangContext.logger('GangService.sendCommandWrapper', {
      type: wrapper.type,
      command: wrapper.command,
      sn,
    });

    return new Promise((resolve) => {
      const sub = this.onCommand.subscribe((c) => {
        if (c.rsn == sn) {
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

  sendState(state: Record<string, unknown>): void {
    if (!this.isHost) throw new Error('only host can send state');

    this.stateSubject.next(state);
    this.send(JSON.stringify(state));

    GangContext.logger('GangService.sendState', state);
  }

  private send(data: string, sn?: number) {
    let a = Uint8Array.from(data, (x) => x.charCodeAt(0));
    if (sn) {
      const sna = Uint32Array.from([sn]);
      a = new Uint8Array([...new Uint8Array(sna.buffer), ...a]);
    }

    this.webSocket.send(a.buffer);
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

function readString(buffer: ArrayBuffer, start = 0, length?: number): string {
  return String.fromCharCode.apply(
    null,
    new Uint8Array(
      length ? buffer.slice(start, start + length) : buffer.slice(start)
    )
  );
}

function readUint32(buffer: ArrayBuffer, start = 0): number {
  return new Uint32Array(buffer.slice(start, start + 4))[0];
}
