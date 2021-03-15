import { Subject, Observable } from 'rxjs';
import { GangWebSocket } from '../../models';
import { parseGangEvent } from './parseGangEvent';
export function createGangWebSocket(url, onOpen, onError, onClose, create = null) {
    const webSocket = !create ? new WebSocket(url) : create(url);
    webSocket.binaryType = 'arraybuffer';
    const observable = new Observable((o) => {
        webSocket.onopen = onOpen;
        webSocket.onmessage = o.next.bind(o);
        webSocket.onerror = (e) => {
            if (onError)
                onError(e);
            o.error(e);
        };
        webSocket.onclose = (e) => {
            if (onClose)
                onClose(e);
            o.complete();
            webSocket.onopen = null;
            webSocket.onmessage = null;
            webSocket.onerror = null;
            webSocket.onclose = null;
            subscription.unsubscribe();
        };
    });
    const subject = new Subject();
    const subscription = observable.subscribe(subject);
    const send = (data) => webSocket.send(data);
    const close = (reason) => webSocket.close(1000, reason);
    return new GangWebSocket(parseGangEvent, subject, send, close);
}
