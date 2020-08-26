import { Subject, Observable, Observer } from 'rxjs';
import { GangWebSocket } from '../models';
import { GangContext } from '../context';

export function createGangWebSocket(
  url: string,
  onOpen: (e: Event) => void,
  onError: (e: Event) => void,
  onClose: (e: CloseEvent) => void
): GangWebSocket {
  GangContext.logger('GangWebSocketFactory.create', url);

  const webSocket = new WebSocket(url);

  const observable = Observable.create((o: Observer<MessageEvent>) => {
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
    };
  });

  const observer = {
    next: (
      data: string | ArrayBuffer | SharedArrayBuffer | Blob | ArrayBufferView
    ) => webSocket.send(data),
  };

  const subject = Subject.create(observer, observable);

  return new GangWebSocket(subject, observer.next, (reason: string) =>
    webSocket.close(1000, reason)
  );
}
