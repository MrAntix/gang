import { Subject, Observable, Observer } from 'rxjs';
import { GangWebSocket } from '../models';
import { GangContext } from '../context';

export interface IWebSocket {
  binaryType: BinaryType;
  onclose: ((this: IWebSocket, ev: CloseEvent) => any) | null;
  onerror: ((this: IWebSocket, ev: Event) => any) | null;
  onmessage: ((this: IWebSocket, ev: MessageEvent) => any) | null;
  onopen: ((this: IWebSocket, ev: Event) => any) | null;
  send(data: string | ArrayBufferLike | Blob | ArrayBufferView): void;
  close(code?: number, reason?: string): void;
}

export function createGangWebSocket(
  url: string,
  onOpen: (e: Event) => void,
  onError: (e: Event) => void,
  onClose: (e: CloseEvent) => void,
  create: (url: string) => IWebSocket = null
): GangWebSocket {
  GangContext.logger('GangWebSocketFactory.create', url);

  const webSocket = !create ? new WebSocket(url) : create(url);
  webSocket.binaryType = 'arraybuffer';

  const observable = new Observable((o: Observer<MessageEvent>) => {
    webSocket.onopen = onOpen;
    webSocket.onmessage = o.next.bind(o);
    webSocket.onerror = (e: Event) => {
      GangContext.logger('GangWebSocketFactory.onerror', e);

      if (onError) onError(e);
      o.error(e);
    };
    webSocket.onclose = (e: CloseEvent) => {
      GangContext.logger('GangWebSocketFactory.onclose', e);

      if (onClose) onClose(e);
      o.complete();

      webSocket.onopen = null;
      webSocket.onmessage = null;
      webSocket.onerror = null;
      webSocket.onclose = null;

      subscription.unsubscribe();
    };
  });

  const subject = new Subject<MessageEvent<any>>();
  const subscription = observable.subscribe(subject)

  const send = (data: string | ArrayBuffer | SharedArrayBuffer | Blob | ArrayBufferView) => webSocket.send(data);
  const close = (reason: string) => webSocket.close(1000, reason);

  return new GangWebSocket(subject, send, close);
}
