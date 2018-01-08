export enum GangConnectionState {
  connecting,
  connected,
  disconnected,
  error
}

export class GangMessage {

  constructor(
    public readonly type: string,
    public readonly content: string) { }
}
