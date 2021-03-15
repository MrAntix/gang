export class GangWebSocket {
    constructor(parse, subject, send, close) {
        this.parse = parse;
        this.subject = subject;
        this.send = send;
        this.close = close;
    }
    subscribe(onMessage, onError, onComplete) {
        return this.subject.subscribe((e) => onMessage(this.parse(e.data)), onError, onComplete);
    }
}
