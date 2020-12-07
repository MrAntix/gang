import { bytesToBase64Url } from '../../services/utils';

export class GangAuthentication {
  constructor(
    public readonly credentialId: string,
    public readonly clientData: string,
    public readonly authenticatorData: string,
    public readonly signature: string
  ) {}

  static from(
    credentialId: string,
    clientDataJSON: ArrayBuffer,
    authenticatorData: ArrayBuffer,
    signature: ArrayBuffer
  ): GangAuthentication {
    return new GangAuthentication(
      credentialId,
      bytesToBase64Url(clientDataJSON),
      bytesToBase64Url(authenticatorData),
      bytesToBase64Url(signature)
    );
  }
}
