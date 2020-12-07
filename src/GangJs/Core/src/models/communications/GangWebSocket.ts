import { Subject, Subscription } from 'rxjs';
import { GangEvents } from './GangEvents';
import { IGangParseEvent } from './IGangParseEvent';

export class GangWebSocket {
  constructor(
    private parse: IGangParseEvent,
    private subject: Subject<MessageEvent>,
    public send: (data: ArrayBuffer) => void,
    public close?: (reason?: string) => void
  ) {}

  subscribe(onMessage: (e: GangEvents) => void, onError?: (e: Event) => void, onComplete?: () => void): Subscription {
    return this.subject.subscribe((e) => onMessage(this.parse(e.data)), onError, onComplete);
  }
}
