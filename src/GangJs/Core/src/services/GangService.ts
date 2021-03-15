import { BehaviorSubject, Subject, Observable, Subscription, ReplaySubject } from 'rxjs';

import { GangContext } from '../context';
import {
  GangWebSocket,
  GangConnectionState,
  GangCommandWrapper,
  NO_RETRY,
  RETRY_INIT,
  RETRY_MAX,
  GangWebSocketFactory,
  IGangCommandSent,
  IGangCommandWaitOptions,
  IGangApplication,
  IGangSettings,
  IGangConnectionProperties,
  GangEventTypes,
} from '../models';
import { GangStore } from './storage';
import { GangUrlBuilder, clean } from './utils';

const GANG_AUTHENTICATION_TOKEN = 'GANG.AUTHENTICATION.TOKEN';
const GANG_STATE = 'GANG.STATE';

export class GangService<TState> {
  private retry = RETRY_INIT;
  private retrying: number;

  retryingIn: number = undefined;
  private webSocket: GangWebSocket;
  private connectionSubject: BehaviorSubject<GangConnectionState>;

  connectionProperties: IGangConnectionProperties;
  get connectionState(): GangConnectionState {
    return this.connectionSubject.value;
  }
  get isConnected(): boolean {
    return this.connectionSubject.value === GangConnectionState.connected;
  }
  memberId: string;
  isAuthenticated: boolean;
  isHost: boolean;

  application: IGangApplication;

  private connectionRetrySubject: Subject<number>;
  private authenticatedSubject: ReplaySubject<string>;
  private memberConnectedSubject: Subject<string>;
  private memberDisconnectedSubject: Subject<string>;
  private commandSubject: Subject<GangCommandWrapper<unknown>>;
  private receiptSubject: Subject<number>;
  private stateSubject: BehaviorSubject<TState>;
  private unsentCommands: GangCommandWrapper<unknown>[] = [];

  onConnection: Observable<GangConnectionState>;
  onConnectionRetry: Observable<number>;
  onAuthenticated: Observable<string>;
  onMemberConnected: Observable<string>;
  onMemberDisconnected: Observable<string>;
  onCommand: Observable<GangCommandWrapper<unknown>>;
  onReceipt: Observable<number>;
  onState: Observable<unknown>;

  constructor(
    private readonly webSocketFactory: GangWebSocketFactory,
    private readonly settings: IGangSettings = GangContext.defaultSettings,
    private readonly initialState: TState = null
  ) {
    this.onConnection = this.connectionSubject = new BehaviorSubject(GangConnectionState.disconnected);
    this.onConnectionRetry = this.connectionRetrySubject = new Subject<number>();
    this.onAuthenticated = this.authenticatedSubject = new ReplaySubject<string>(1);
    this.onMemberConnected = this.memberConnectedSubject = new Subject<string>();
    this.onMemberDisconnected = this.memberDisconnectedSubject = new Subject<string>();
    this.onCommand = this.commandSubject = new Subject<GangCommandWrapper<Record<string, unknown>>>();
    this.onReceipt = this.receiptSubject = new Subject<number>();
    this.onState = this.stateSubject = new BehaviorSubject<TState>(initialState);
  }

  /**
   * Connect to gang
   */
  async connect(properties?: IGangConnectionProperties): Promise<boolean> {
    if (this.isConnected) await this.disconnect('reconnect');

    this.connectionProperties = {
      ...this.connectionProperties,
      token: GangStore.get(GANG_AUTHENTICATION_TOKEN),
      ...clean(properties),
    };
    GangContext.logger('GangService.connect', this.connectionProperties);

    return new Promise<boolean>((resolve) => {
      this.connectionSubject.next(GangConnectionState.connecting);
      const connectUrl = GangUrlBuilder.from(this.settings.rootUrl + this.connectionProperties.path)
        .set('gangId', this.connectionProperties.gangId)
        .set('token', this.connectionProperties.token)
        .build();
      GangContext.logger('GangService.connected', this.settings.rootUrl + this.connectionProperties.path, connectUrl);

      this.webSocket = this.webSocketFactory(
        connectUrl,
        (e: Event) => {
          GangContext.logger('GangService.onopen', e);

          this.connectionSubject.next(GangConnectionState.connected);

          this.retry = RETRY_INIT;
          this.retryingIn = undefined;
          resolve(true);

          let wrapper: GangCommandWrapper<unknown>;
          while ((wrapper = this.unsentCommands.shift())) this.sendCommandWrapper(wrapper);

          window.removeEventListener('online', this.online);

          this.offline = () => {
            GangContext.logger('GangService.offline');
            this.isConnected && this.disconnect(null);
          };
          window.addEventListener('offline', this.offline);

          clearRetryConnect();
        },
        (e: Event) => {
          GangContext.logger('GangService.onerror', e);

          this.connectionSubject.next(GangConnectionState.error);
          resolve(false);
        },
        (e: CloseEvent) => {
          GangContext.logger('GangService.onclose', e);

          this.connectionSubject.next(GangConnectionState.disconnected);

          window.removeEventListener('offline', this.offline);

          if (!e.reason) {
            this.online = () => {
              GangContext.logger('GangService.online');
              !this.isConnected &&
                this.connect(properties).catch(() => {
                  // do nothing.
                });
            };

            window.addEventListener('online', this.online);

            retryConnect();
          }
        }
      );

      this.webSocket.subscribe((e) => {
        GangContext.logger('GangService.webSocket.message', e);

        switch (e.type) {
          case GangEventTypes.Host:
            this.isHost = true;
            this.memberId = e.auth.memberId;
            this.application = e.auth.application;
            this.authenticate(e.auth.token);
            break;

          case GangEventTypes.Member:
            this.isHost = false;
            this.memberId = e.auth.memberId;
            this.application = e.auth.application;
            this.authenticate(e.auth.token);
            break;

          case GangEventTypes.Denied:
            this.memberId = e.auth.memberId;
            this.application = e.auth.application;
            this.authenticate(e.auth.token);
            break;

          case GangEventTypes.MemberConnected:
            this.memberConnectedSubject.next(e.memberId);
            break;

          case GangEventTypes.MemberDisconnected:
            this.memberDisconnectedSubject.next(e.memberId);
            break;

          case GangEventTypes.Command:
            this.commandSubject.next(e.wrapper);
            break;

          case GangEventTypes.CommandReceipt:
            this.receiptSubject.next(e.rsn);
            this.unsentCommands = this.unsentCommands.filter((w) => w.sn !== e.rsn);
            break;

          case GangEventTypes.State:
            this.stateSubject.next({
              ...this.stateSubject.value,
              ...e.state,
            });
            break;
        }
      });

      const retryConnect = (() => {
        if (this.retry === NO_RETRY || this.retrying || this.isConnected) return;

        GangContext.logger('GangService.retryConnect in', this.retry);

        this.retryingIn = this.retry;
        this.retrying = window.setInterval(() => {
          this.retryingIn--;
          this.connectionRetrySubject.next(this.retryingIn);

          if (this.retryingIn === 0) {
            clearRetryConnect();
            this.connect(properties).catch(() => {
              // do nothing.
            });
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

  authenticate(token: string): void {
    GangStore.set(GANG_AUTHENTICATION_TOKEN, token);

    this.isAuthenticated = !!token;
    this.authenticatedSubject.next(token);

    let state = this.initialState;
    if (this.isAuthenticated) {
      const stateJson = GangStore.get(GANG_STATE);
      if (stateJson) state = JSON.parse(stateJson);
    }

    this.stateSubject.next(state);
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

  private offline: () => void;
  private online: () => void;

  /** Set the local current state, ie not sent to the server
   *
   * @param state the passed state will be shallow merged with the current state
   */
  setState(state: Partial<TState>): void {
    GangStore.set(GANG_STATE, JSON.stringify(state));

    this.stateSubject.next({
      ...this.stateSubject.value,
      ...state,
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
    onGangConnectionRetry?: (seconds: number) => void;
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
      'ConnectionRetry',
    ].forEach((key) => {
      const componentKey = `onGang${key}`;
      const serviceKey = `on${key}`;

      if (component[componentKey]) subs.push(this[serviceKey].subscribe((e: unknown) => component[componentKey](e)));
      else if (component[serviceKey] !== undefined)
        console.warn(`${serviceKey} changed to ${componentKey}, please update your code`, component);
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
  mapActions<C extends { [K in keyof A]: unknown }, A>(component: C, actions: A): void {
    Object.keys(actions).forEach((key) => {
      component[key] = actions[key](this);
    });
  }

  /**
   * Executes a command locally no data is sent to the host
   *
   * @param type Command type name
   * @param data Command data
   */
  executeCommand<T>(type: string, data: T): void {
    const wrapper = new GangCommandWrapper(type, data);
    GangContext.logger('GangService.executeCommand', {
      wrapper,
      isConnected: this.isConnected,
    });

    this.commandSubject.next(wrapper);
  }

  private sn = 0;

  /**
   * Sends a command to the host member
   * await this if you expect a reply command from the host
   *
   * @param type Command type name
   * @param data Command data
   *
   * @returns a IGangCommandSent
   */
  sendCommand<T>(type: string, data: T): IGangCommandSent {
    const sn = ++this.sn;
    const wrapper = new GangCommandWrapper(type, data, sn);
    GangContext.logger('GangService.sendCommand', {
      wrapper,
      isConnected: this.isConnected,
    });

    this.unsentCommands = [...this.unsentCommands, wrapper];

    if (!this.isConnected) return;

    if (this.isHost) {
      this.commandSubject.next(wrapper);
      return;
    }

    return this.sendCommandWrapper(wrapper);
  }

  private sendCommandWrapper<T>(wrapper: GangCommandWrapper<T>): IGangCommandSent {
    this.send(
      JSON.stringify({
        type: wrapper.type,
        data: wrapper.data,
      }),
      wrapper.sn
    );

    GangContext.logger('GangService.sendCommandWrapper', wrapper);

    return {
      sn: wrapper.sn,
      wait: (options: IGangCommandWaitOptions) =>
        new Promise((resolve) => {
          const sub = this.onCommand.subscribe((w) => {
            if (w.rsn == wrapper.sn) {
              sub.unsubscribe();
              resolve(w);
            }
          });

          setTimeout(() => {
            sub.unsubscribe();
            resolve(null);
          }, options?.timeout || 30000);
        }),
    };
  }

  sendState(state: TState): void {
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

    try {
      this.webSocket.send(a.buffer);
    } catch (err) {
      GangContext.logger('GangService.send error', err);
    }
  }

  waitForCommand<T>(
    type: string,
    predicate: (c: T) => boolean,
    options?: {
      timeout?: number;
    }
  ): Promise<void> {
    const test: (c: GangCommandWrapper<unknown>) => boolean = (c) => {
      return (!type || type === c.type) && (!predicate || predicate(c.data as T));
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

  static setState(value: string): void {
    GangStore.set(GANG_STATE, value);
  }
}
