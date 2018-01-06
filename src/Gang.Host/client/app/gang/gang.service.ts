import { Injectable } from '@angular/core'

import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/first';

import { WebSocketState } from './gang.contracts';

const NO_RETRY = -1;
const RETRY_INIT = 5;
const RETRY_MAX = 40;

@Injectable()
export class GangService {

  private readonly rootUrl: string;
  private retry = RETRY_INIT;
  private retrying: any;
  private retryingIn: number = undefined;

  private socket: WebSocket;
  state: WebSocketState;

  onConnect: Subject<void>;
  onMessage: Subject<any>;

  constructor() {

    const location = window.location;
    const protocol = location.protocol.replace('http', 'ws');
    this.rootUrl = `${protocol}//${location.host}/`;

    this.onConnect = new Subject<void>();
    this.onMessage = new Subject<any>();
  }

  connect(url: string, gangId: string): void {

    this.state = WebSocketState.connecting;

    const connectUrl =  this.rootUrl + url + `?gangId=${gangId}`;

    console.debug('GangService.connect',);

    this.socket = new WebSocket(connectUrl);

    this.socket.onopen = ((e: Event) => {
      console.debug('GangService.onopen', e);

      this.state = WebSocketState.connected;
      this.retry = RETRY_INIT;
      this.retryingIn = undefined;

      clearRetryConnect();

      this.onConnect.next();

    }).bind(this);

    this.socket.onmessage = ((e: MessageEvent) => {

      this.onMessage.next(e.data);

    }).bind(this);

    this.socket.onclose = ((e: Event) => {
      console.debug('GangService.onclose', e);

      this.state = WebSocketState.disconnected;
      retryConnect();

    }).bind(this);

    this.socket.onerror = ((e: Event) => {
      console.error('GangService.onerror', e);

      this.state = WebSocketState.error;
      retryConnect();
    }).bind(this);

    const retryConnect = (() => {
      if (this.retry === NO_RETRY || this.retrying || this.socket.readyState === 1) return;

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

}
