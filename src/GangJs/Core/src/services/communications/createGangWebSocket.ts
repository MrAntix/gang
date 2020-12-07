import { Subject, Observable, Observer } from 'rxjs';
import { GangWebSocket, IWebSocket, WebSocketData } from '../../models';
import { parseGangEvent } from './parseGangEvent';

export function createGangWebSocket(
  url: string,
  onOpen: (e: Event) => void,
  onError: (e: Event) => void,
  onClose: (e: CloseEvent) => void,
  create: (url: string) => IWebSocket = null
): GangWebSocket {
  const webSocket = !create ? new WebSocket(url) : create(url);
  webSocket.binaryType = 'arraybuffer';

  const observable = new Observable((o: Observer<MessageEvent>) => {
    webSocket.onopen = onOpen;
    webSocket.onmessage = o.next.bind(o);
    webSocket.onerror = (e: Event) => {
      if (onError) onError(e);
      o.error(e);
    };
    webSocket.onclose = (e: CloseEvent) => {
      if (onClose) onClose(e);
      o.complete();

      webSocket.onopen = null;
      webSocket.onmessage = null;
      webSocket.onerror = null;
      webSocket.onclose = null;

      subscription.unsubscribe();
    };
  });

  const subject = new Subject<MessageEvent<unknown>>();
  const subscription = observable.subscribe(subject);

  const send = (data: WebSocketData) => webSocket.send(data);
  const close = (reason: string) => webSocket.close(1000, reason);

  return new GangWebSocket(parseGangEvent, subject, send, close);
}
