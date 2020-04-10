import { BehaviorSubject, Subject, Observable } from "rxjs";

import {
  GangWebSocket,
  GangConnectionState,
  GangCommandWrapper,
  GangUrlBuilder,
  NO_RETRY,
  RETRY_INIT,
  RETRY_MAX,
  IGangWebSocketFactory,
} from "../models";

export class GangService {
  private readonly rootUrl: string;
  private retry = RETRY_INIT;
  private retrying: any;

  retryingIn: number = undefined;
  private webSocket: GangWebSocket;
  private connectionState: GangConnectionState;

  get state() {
    return this.connectionState;
  }
  get isConnected() {
    return this.connectionState === GangConnectionState.connected;
  }
  memberId: string;
  isHost: boolean;

  private memberConnectedSubject: Subject<string>;
  private memberDisconnectedSubject: Subject<string>;
  private commandSubject: Subject<any>;
  private stateSubject: BehaviorSubject<any>;
  private lastState: any;
  private unsentCommands: GangCommandWrapper[] = [];

  onMemberConnected: Observable<string>;
  onMemberDisconnected: Observable<string>;
  onCommand: Observable<any>;
  onState: Observable<any>;

  constructor(
    private webSocketFactory: IGangWebSocketFactory,
    defaultState: any = {}) {
    if (!location) throw "required location object not found";

    const protocol = location.protocol.replace("http", "ws");
    const host = location.host;
    this.rootUrl = `${protocol}//${host}/`;

    this.onMemberConnected = this.memberConnectedSubject = new Subject<string>();
    this.onMemberDisconnected = this.memberDisconnectedSubject = new Subject<string>();
    this.onCommand = this.commandSubject = new Subject<any>();
    this.onState = this.stateSubject = new BehaviorSubject<any>(defaultState);
  }

  connect(url: string, gangId: string, token?: string): void {
    this.connectionState = GangConnectionState.connecting;
    const connectUrl = GangUrlBuilder
      .from(this.rootUrl + url)
      .set("gangId", gangId)
      .set("token", token)
      .build();
    console.debug("GangService.connect", this.rootUrl + url, connectUrl);

    this.webSocket = this.webSocketFactory(
      connectUrl,
      (e: Event) => {
        console.debug("GangService.onopen", e);

        this.connectionState = GangConnectionState.connected;
        this.retry = RETRY_INIT;
        this.retryingIn = undefined;

        if (this.lastState) {
          this.stateSubject.next(this.lastState);
          let wrapper;
          while ((wrapper = this.unsentCommands.shift()))
            this.sendCommandWrapper(wrapper);
        }

        clearRetryConnect();
      },
      (e: Event) => {
        console.error("GangService.onerror", e);

        this.connectionState = GangConnectionState.error;
      },
      (e: CloseEvent) => {
        console.debug("GangService.onclose");

        this.connectionState = GangConnectionState.disconnected;
        this.memberDisconnectedSubject.next(this.memberId);

        if (!e.reason) retryConnect();
      }
    );

    this.webSocket.subscribe((e: MessageEvent) => {
      var reader = new FileReader();

      reader.onload = () => {
        const messageType = reader.result[0];
        const messageData = reader.result.slice(1) as string;

        console.debug(
          "GangService.onmessage type:", messageType,
          "data:", messageData
        );

        switch (messageType) {
          default:
            throw `unknown message type: ${messageType}`;
          case "H":
            this.isHost = true;
            this.memberId = messageData;
            this.memberConnectedSubject.next(messageData);
            break;
          case "M":
            this.isHost = false;
            this.memberId = messageData;
            this.memberConnectedSubject.next(messageData);
            break;
          case "+":
            this.memberConnectedSubject.next(messageData);
            break;
          case "-":
            this.memberDisconnectedSubject.next(messageData);
            break;
          case "C":
            var command = JSON.parse(messageData);
            this.commandSubject.next(command);
            break;
          case "S":
            var state = JSON.parse(messageData);
            this.stateSubject.next(state);
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
      console.debug("GangService.retryConnect in", this.retry);
      this.retryingIn = this.retry;
      this.retrying = setInterval(() => {
        this.retryingIn--;
        if (this.retryingIn === 0) {
          clearRetryConnect();
          this.connect(url, gangId);
        }
      }, 1000);
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
    var wrapper = new GangCommandWrapper(type, command);
    console.debug("GangService.sendCommand", wrapper);
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

  sendState(state: any): void {
    if (!this.isHost) throw "only host can send state";
    if (this.isConnected) this.lastState = state;
    this.stateSubject.next(state);
    this.send(state);
  }

  private send(data: any): void {
    var blob = new Blob([JSON.stringify(data)], {
      type: "text/plain",
    });
    this.webSocket.send(blob);
  }
}
