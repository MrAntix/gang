import { GangPublicKey } from './GangPublicKey';
export declare class GangCredentialRegistration {
    readonly credentialId: string;
    readonly publicKey: GangPublicKey;
    readonly transports: string[];
    readonly challenge: string;
    constructor(credentialId: string, publicKey: GangPublicKey, transports: string[], challenge: string);
    static from(authData: Uint8Array, transports: string[], challenge: ArrayBuffer): GangCredentialRegistration;
}
