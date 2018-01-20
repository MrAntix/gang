import { Injectable } from '@angular/core'

import * as Rx from 'rxjs/Rx';
import 'rxjs/add/operator/first';

import { GangConnectionState, GangUrlBuilder } from './gang.contracts';
import { GangWebSocketFactory, GangWebSocket } from './gang.webSocket.factory';

const NO_RETRY = -1;
const RETRY_INIT = 5;
const RETRY_MAX = 40;

@Injectable()
export class GangService {

  private readonly rootUrl: string;
  private retry = RETRY_INIT;
  private retrying: any;
  retryingIn: number = undefined;

  private webSocket: GangWebSocket;
  state: GangConnectionState;
  get isConnected() { return this.state === GangConnectionState.connected; }

  memberId: string;
  isHost: boolean;

  private memberConnectSubject: Rx.Subject<string>;
  private memberDisconnectSubject: Rx.Subject<string>;
  private commandSubject: Rx.Subject<any>;
  private stateSubject: Rx.Subject<any>;

  onMemberConnect: Rx.Observable<string>;
  onMemberDisconnect: Rx.Observable<string>;
  onCommand: Rx.Observable<any>;
  onState: Rx.Observable<any>;

  constructor(
    private webSocketFactory: GangWebSocketFactory) {

    if (!location) throw 'required location object not found';

    const protocol = location.protocol.replace('http', 'ws');
    const host = location.host;
    this.rootUrl = `${protocol}//${host}/`;

    this.onMemberConnect = this.memberConnectSubject = new Rx.Subject<string>();
    this.onMemberDisconnect = this.memberDisconnectSubject = new Rx.Subject<string>();
    this.onCommand = this.commandSubject = new Rx.Subject<any>();
    this.onState = this.stateSubject = new Rx.Subject<any>();
  }

  connect(url: string, gangId: string, token?: string): void {

    this.state = GangConnectionState.connecting;

    const connectUrl = GangUrlBuilder
      .from(this.rootUrl + url)
      .set('gangId', gangId)
      .set('token', token)
      .build();

    console.debug('GangService.connect', this.rootUrl + url, connectUrl);

    this.webSocket = this.webSocketFactory.create(
      connectUrl,
      (e: Event) => {
        console.debug('GangService.onopen', e);

        this.state = GangConnectionState.connected;
        this.retry = RETRY_INIT;
        this.retryingIn = undefined;

        clearRetryConnect();

      },
      (e: Event) => {
        console.error('GangService.onerror', e);

        this.state = GangConnectionState.error;
      },
      (e: CloseEvent) => {
        console.debug('GangService.onclose');

        this.state = GangConnectionState.disconnected;
        this.memberDisconnectSubject.next(this.memberId);

        if (!e.reason) retryConnect();

      });

    this.webSocket
      .subscribe(
      (e: MessageEvent) => {

        var reader = new FileReader();
        reader.onload = () => {

          const messageType = reader.result[0];
          const messageData = reader.result.slice(1);
          console.debug('GangService.onmessage type:', messageType, 'data:', messageData);

          switch (messageType) {
            default: throw `unknown message type: ${messageType}`;
            case 'H':
              this.isHost = true;
              this.memberId = messageData;
              this.memberConnectSubject.next(this.memberId);

              break;
            case 'M':
              this.isHost = false;
              this.memberId = messageData;
              this.memberConnectSubject.next(this.memberId);

              break;
            case 'D':
              this.isHost = true;
              this.memberDisconnectSubject.next(messageData);

              break;
            case 'C':
              var command = JSON.parse(messageData);
              this.commandSubject.next(command);

              break;
            case 'S':
              var state = JSON.parse(messageData);
              this.stateSubject.next(state);

              break;
          }
        };

        reader.readAsText(e.data);
      });

    const retryConnect = (() => {
      if (this.retry === NO_RETRY
        || this.retrying
        || this.state === GangConnectionState.connected) return;

      console.debug('GangService.retryConnect in', this.retry);
      this.retryingIn = this.retry;
      this.retrying = setInterval(() => {

        this.retryingIn--;
        if (this.retryingIn === 0) {
          clearRetryConnect();
          this.connect(url, gangId);
        }
      },
        1000);

      if (this.retry < RETRY_MAX) this.retry *= 2;

    }).bind(this);

    const clearRetryConnect = (() => {
      if (this.retrying) {

        clearInterval(this.retrying);
        this.retrying = 0;
      }
    }).bind(this);

    retryConnect();
  }

  sendCommand(type: string, command: any): void {

    var wrapper = {
      type: type,
      command: command
    };

    if (this.isHost) {
      this.commandSubject.next(wrapper);

      return;
    }

    this.send(wrapper);
  }

  sendState(state: any): void {

    if (!this.isHost) throw 'only host can send state';

    this.stateSubject.next(state);
    this.send(state);
  }

  private send(data: any): void {

    var blob = new Blob(
      [JSON.stringify(data)],
      {
        type: 'text/plain'
      });

    this.webSocket.send(blob);
  }
}
