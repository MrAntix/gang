import { Subject, Subscription } from 'rxjs';
import { GangEvents } from './GangEvents';
import { IGangParseEvent } from './IGangParseEvent';
export declare class GangWebSocket {
    private parse;
    private subject;
    send: (data: ArrayBuffer) => void;
    close?: (reason?: string) => void;
    constructor(parse: IGangParseEvent, subject: Subject<MessageEvent>, send: (data: ArrayBuffer) => void, close?: (reason?: string) => void);
    subscribe(onMessage: (e: GangEvents) => void, onError?: (e: Event) => void, onComplete?: () => void): Subscription;
}
