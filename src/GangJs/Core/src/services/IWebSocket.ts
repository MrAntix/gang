
export interface IWebSocket {
  binaryType: BinaryType;
  onclose: ((this: IWebSocket, ev: CloseEvent) => any) | null;
  onerror: ((this: IWebSocket, ev: Event) => any) | null;
  onmessage: ((this: IWebSocket, ev: MessageEvent) => any) | null;
  onopen: ((this: IWebSocket, ev: Event) => any) | null;
  send(data: string | ArrayBufferLike | Blob | ArrayBufferView): void;
  close(code?: number, reason?: string): void;
}
