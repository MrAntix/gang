export class GangAuthenticationService {

  async register(
    app: IGangAuthenticationApplication,
    user: IGangAuthenticationUser):
    Promise<Credential> {

    const publicKeyCredentialCreationOptions: PublicKeyCredentialCreationOptions
      = {
      challenge: Uint8Array.from(app.challenge, c => c.charCodeAt(0)),
      rp: {
        name: app.name,
        id: app.id
      },
      user: {
        id: Uint8Array.from(user.id, c => c.charCodeAt(0)),
        name: user.emailAddress,
        displayName: user.name,
      },
      pubKeyCredParams: [{ alg: -7, type: "public-key" }],
      authenticatorSelection: {
        authenticatorAttachment: "cross-platform",
      },
      timeout: 60000,
      attestation: "direct"
    };

    return await navigator.credentials.create({
      publicKey: publicKeyCredentialCreationOptions
    });
  }
}

export interface IGangAuthenticationApplication {
  id: string;
  name: string;
  challenge: string;
}

export interface IGangAuthenticationUser {
  id: string;
  name: string;
  emailAddress: string;
}
