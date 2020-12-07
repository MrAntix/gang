import { GangData } from '../GangData';

export class GangAuthenticationCredential {
  constructor(public readonly id: string, public readonly transports: AuthenticatorTransport[]) {}

  static from(data: GangData<GangAuthenticationCredential>): GangAuthenticationCredential {
    return new GangAuthenticationCredential(data.id, data.transports);
  }
}
