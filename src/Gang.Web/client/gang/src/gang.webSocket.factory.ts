import { Injectable } from '@angular/core';

import { Observable, Observer, Subject, Subscription } from 'rxjs';

@Injectable()
export class GangWebSocketFactory {

  create(
    url: string,
    onOpen: (e: Event) => void,
    onError: (e: Event) => void,
    onClose: (e: CloseEvent) => void
  ): GangWebSocket {

    console.debug('GangWebSocketFactory.create', url);

    const webSocket = new WebSocket(url);

    const observable = Observable
      .create((o: Observer<MessageEvent>) => {

        webSocket.onopen = onOpen;
        webSocket.onmessage = o.next.bind(o);
        webSocket.onerror = (e: Event) => {

          if (onError) onError(e);
          o.error.bind(o);
        };
        webSocket.onclose = (e: CloseEvent) => {

          if (onClose) onClose(e);
          o.complete.bind(o);
        };

        return webSocket.close.bind(webSocket);
      });

    const observer = {
      next: (data: any) => webSocket.send(data)
    }

    const subject = Subject.create(observer, observable);

    return new GangWebSocket(subject, observer.next);
  }
}

export class GangWebSocket {

  constructor(
    private subject: Subject<MessageEvent>,
    public send: (data: Object) => void
  ) { }

  subscribe(
    onMessage: (e: MessageEvent) => void,
    onError?: (e: Event) => void,
    onComplete?: () => void
  ): Subscription {

    return this.subject.subscribe(onMessage, onError, onComplete);
  }
}
