import { bytesToBase64Url } from '../../services/utils';
export class GangAuthentication {
    constructor(credentialId, clientData, authenticatorData, signature) {
        this.credentialId = credentialId;
        this.clientData = clientData;
        this.authenticatorData = authenticatorData;
        this.signature = signature;
    }
    static from(credentialId, clientDataJSON, authenticatorData, signature) {
        return new GangAuthentication(credentialId, bytesToBase64Url(clientDataJSON), bytesToBase64Url(authenticatorData), bytesToBase64Url(signature));
    }
}
