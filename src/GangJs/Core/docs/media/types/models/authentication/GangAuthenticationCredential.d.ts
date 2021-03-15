import { GangData } from '../GangData';
export declare class GangAuthenticationCredential {
    readonly id: string;
    readonly transports: AuthenticatorTransport[];
    constructor(id: string, transports: AuthenticatorTransport[]);
    static from(data: GangData<GangAuthenticationCredential>): GangAuthenticationCredential;
}
