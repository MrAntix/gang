import { Subject, Subscription } from 'rxjs';

export class GangWebSocket {
  constructor(
    private subject: Subject<MessageEvent>,
    public send: (data: Object) => void) { }

  subscribe(
    onMessage: (e: MessageEvent) => void,
    onError?: (e: Event) => void,
    onComplete?: () => void): Subscription {

    return this.subject.subscribe(onMessage, onError, onComplete);
  }
}
