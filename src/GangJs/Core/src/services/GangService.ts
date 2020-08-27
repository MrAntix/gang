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
  private commandSubject: Subject<unknown>;
  private stateSubject: BehaviorSubject<unknown>;
  private unsentCommands: GangCommandWrapper[] = [];

  onConnection: Observable<GangConnectionState>;
  onMemberConnected: Observable<string>;
  onMemberDisconnected: Observable<string>;
  onCommand: Observable<unknown>;
  onState: Observable<unknown>;

  constructor(
    private webSocketFactory: GangWebSocketFactory) {
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
    this.onCommand = this.commandSubject = new Subject<unknown>();
    this.onState = this.stateSubject = new BehaviorSubject<unknown>(undefined);
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

          let wrapper: GangCommandWrapper;
          while ((wrapper = this.unsentCommands.shift())) this.send(wrapper);

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
              this.stateSubject.next(JSON.parse(messageData));
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

  sendCommand(type: string, command: unknown): void {
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

    this.sendCommandWrapper(wrapper);
  }

  private sendCommandWrapper(wrapper: GangCommandWrapper) {
    if (this.isHost) {
      this.commandSubject.next(wrapper);
      return;
    }
    this.send(wrapper);
  }

  sendState(state: unknown): void {
    if (!this.isHost) throw new Error('only host can send state');

    this.stateSubject.next(state);
    this.send(state);
  }

  private send(data: unknown): void {
    const blob = new Blob([JSON.stringify(data)], {
      type: 'text/plain',
    });
    this.webSocket.send(blob);

    GangContext.logger('GangService.send', {
      data,
    });
  }
}
