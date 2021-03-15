import { bytesToBase64Url, bytesToString, viewToBuffer } from '../../services/utils';
import { GangPublicKey } from './GangPublicKey';
export class GangCredentialRegistration {
    constructor(credentialId, publicKey, transports, challenge) {
        this.credentialId = credentialId;
        this.publicKey = publicKey;
        this.transports = transports;
        this.challenge = challenge;
    }
    static from(authData, transports, challenge) {
        const buffer = viewToBuffer(authData);
        const idLength = new DataView(buffer).getInt16(53);
        return new GangCredentialRegistration(bytesToBase64Url(buffer.slice(55, 55 + idLength)), GangPublicKey.from(buffer.slice(55 + idLength)), transports, bytesToString(challenge));
    }
}
