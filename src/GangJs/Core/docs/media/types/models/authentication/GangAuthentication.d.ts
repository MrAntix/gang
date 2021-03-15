export declare class GangAuthentication {
    readonly credentialId: string;
    readonly clientData: string;
    readonly authenticatorData: string;
    readonly signature: string;
    constructor(credentialId: string, clientData: string, authenticatorData: string, signature: string);
    static from(credentialId: string, clientDataJSON: ArrayBuffer, authenticatorData: ArrayBuffer, signature: ArrayBuffer): GangAuthentication;
}
