import {
  IGangHttp,
  IGangLocationService,
  IGangCredentialsService,
  GangUrlBuilder, IGangTokenData, IGangSettings, IGangPlatform
} from '../models';

export class GangAuthenticationService {

  /**
   * Create a service
   * @param settings
   * @param http
   * @param location
   * @param credentials
   */
  constructor(
    private settings: IGangSettings,
    private http: IGangHttp,
    private location: IGangLocationService,
    private credentials: IGangCredentialsService
  ) {

    if (window.PublicKeyCredential)

      this.platform = PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable()
        .then(value => ({
          hasAuthenticator: value
        }));

    else
      this.platform = Promise.resolve({
        hasAuthenticator: false
      });
  }

  /**
  * Request a link code
  *
  * @param email email address
  */
  async requestLink(email: string): Promise<boolean> {
    const result = await this.http.fetch(`/request-link`, {
      method: 'POST',
      headers: {
        'Content-type': 'application/json',
      },
      body: `"${email}"`,
    });

    return result.ok;
  }

  /**
   * Checks the url for a link code
   *
   * if found, it will be used to fetch a session token and removed from the url
   *
   * @param {string} [parameterName=link-code] - name of the url parameter
   */
  async tryGetTokenFromUrl(parameterName = 'link-code'): Promise<string> {

    const urlBuilder = new GangUrlBuilder(this.location.href);
    const code = urlBuilder.parameters[parameterName];

    if (code) {
      // code found, remove from the url and link
      delete urlBuilder.parameters[parameterName];
      this.location.pushState(urlBuilder.build());

      return await this.tryGetToken(code[0]);
    }

    return undefined;
  }

  /**
   * Attempts to get a session token, given a code
   *
   * @param {string} [code]
   */
  async tryGetToken(code: string): Promise<string> {

    const result = await this.http.fetch(`/link/${code}`);
    if (!result.ok) return undefined;

    return await result.text();
  }

  readonly platform: Promise<IGangPlatform>;

  async register(
    tokenData: IGangTokenData,
    challenge: string): Promise<PublicKeyCredential> {

    const publicKey: PublicKeyCredentialCreationOptions = {
      challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
      rp: {
        name: this.settings.app.name,
      },
      user: {
        id: Uint8Array.from(tokenData.id, c => c.charCodeAt(0)),
        name: tokenData.emailAddress,
        displayName: tokenData.name || tokenData.emailAddress,
      },
      pubKeyCredParams: [{ alg: -7, type: 'public-key' }],
      authenticatorSelection: {
        userVerification: 'required',
        requireResidentKey: true
      },
      timeout: 60000,
      attestation: 'none'
    };

    return await this
      .credentials.create({ publicKey }) as PublicKeyCredential;
  }

  async getCredential(
    _tokenData: IGangTokenData,
    challenge: string
  ): Promise<PublicKeyCredential> {

    const publicKey: PublicKeyCredentialRequestOptions = {
      challenge: Uint8Array.from(challenge, c => c.charCodeAt(0)),
      userVerification: 'required',
      timeout: 60000
    };

    return await this
      .credentials.get({ publicKey }) as PublicKeyCredential;
  }

  /**
   * decode a token to data
   *
   * @param token valid token
   */
  decodeToken(token: string): IGangTokenData {
    if (!token)
      return undefined;

    const tokenParts = token.split('.');
    if (tokenParts.length != 2)
      return undefined;

    const data = tokenParts[0];

    return JSON.parse(atob(data)) as IGangTokenData;
  }
}
