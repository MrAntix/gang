import { bytesToBase64Url, bytesToString, viewToBuffer } from '../../services/utils';
import { GangPublicKey } from './GangPublicKey';

export class GangCredentialRegistration {
  constructor(
    public readonly credentialId: string,
    public readonly publicKey: GangPublicKey,
    public readonly transports: string[],
    public readonly challenge: string
  ) {}

  static from(authData: Uint8Array, transports: string[], challenge: ArrayBuffer): GangCredentialRegistration {
    const buffer = viewToBuffer(authData);
    const idLength = new DataView(buffer).getInt16(53);

    return new GangCredentialRegistration(
      bytesToBase64Url(buffer.slice(55, 55 + idLength)),
      GangPublicKey.from(buffer.slice(55 + idLength)),
      transports,
      bytesToString(challenge)
    );
  }
}
