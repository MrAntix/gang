export class GangAuthenticationCredential {
    constructor(id, transports) {
        this.id = id;
        this.transports = transports;
    }
    static from(data) {
        return new GangAuthenticationCredential(data.id, data.transports);
    }
}
