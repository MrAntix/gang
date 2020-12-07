import { GangUrlBuilder, IGangHttp, IGangLocationService, IGangTokenData } from '../models';

export class GangAuthenticationService {

  /**
   * Create a service
   * @param settings
   * @param location
   */
  constructor(
    private http: IGangHttp,
    private location: IGangLocationService
  ) { }

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
