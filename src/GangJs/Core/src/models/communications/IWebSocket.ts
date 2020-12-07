import { WebSocketData } from './WebSocketData';

export interface IWebSocket {
  binaryType: BinaryType;
  onclose: ((this: IWebSocket, ev: CloseEvent) => void) | null;
  onerror: ((this: IWebSocket, ev: Event) => void) | null;
  onmessage: ((this: IWebSocket, ev: MessageEvent) => void) | null;
  onopen: ((this: IWebSocket, ev: Event) => void) | null;
  send(data: WebSocketData): void;
  close(code?: number, reason?: string): void;
}
