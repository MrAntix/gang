import { Injectable } from '@angular/core'

import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/first';

import { GangConnectionState } from './gang.contracts';

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
  state: GangConnectionState;
  id: string;
  isHost: boolean;

  onConnect: Subject<string>;
  onDisconnect: Subject<string>;
  onCommand: Subject<any>;
  onState: Subject<any>;

  constructor() {

    const location = window.location;
    const protocol = location.protocol.replace('http', 'ws');
    this.rootUrl = `${protocol}//${location.host}/`;

    this.onConnect = new Subject<string>();
    this.onDisconnect = new Subject<string>();
    this.onCommand = new Subject<string>();
    this.onState = new Subject<string>();
  }

  connect(url: string, gangId: string): void {

    this.state = GangConnectionState.connecting;

    const connectUrl = this.rootUrl + url + `?gangId=${gangId}`;

    console.debug('GangService.connect');

    this.socket = new WebSocket(connectUrl);

    this.socket.onopen = ((e: Event) => {
      console.debug('GangService.onopen', e);

      this.state = GangConnectionState.connected;
      this.retry = RETRY_INIT;
      this.retryingIn = undefined;

      clearRetryConnect();

    }).bind(this);

    this.socket.onmessage = ((e: MessageEvent) => {

      var reader = new FileReader();
      reader.onload = () => {
        console.debug('GangService.onmessage', reader.result);

        switch (reader.result[0]) {
          case 'H':
            this.isHost = true;
            this.id = reader.result.slice(1);
            this.onConnect.next(this.id);

            break;
          case 'M':
            this.isHost = false;
            this.id = reader.result.slice(1);
            this.onConnect.next(this.id);

            break;
          case 'D':
            this.isHost = true;
            this.onDisconnect.next(reader.result.slice(1));

            break;
          case 'C':
            var command = JSON.parse(reader.result.slice(1));
            this.onCommand.next(command);

            break;
          case 'S':
            var state = JSON.parse(reader.result.slice(1));
            this.onState.next(state);

            break;
        }
      };

      reader.readAsText(e.data);

    }).bind(this);

    this.socket.onclose = ((e: Event) => {
      console.debug('GangService.onclose', e);

      this.state = GangConnectionState.disconnected;
      this.onDisconnect.next(this.id);

      retryConnect();

    }).bind(this);

    this.socket.onerror = ((e: Event) => {
      console.error('GangService.onerror', e);

      this.state = GangConnectionState.error;
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

  sendCommand(type: string, command: any): void {

    var wrapper = {
      type: type,
      command: command
    };

    if (this.isHost) {
      this.onCommand.next(wrapper);

      return;
    }

    this.send(wrapper);
  }

  sendState(state: any): void {

    if (!this.isHost) throw 'only host can send state';

    this.onState.next(state);
    this.send(state);
  }

  private send(data: any): void {

    var blob = new Blob(
      [JSON.stringify(data)],
      {
        type: 'text/plain'
      });

    this.socket.send(blob);
  }
}
