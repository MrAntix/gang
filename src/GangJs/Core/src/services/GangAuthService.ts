import { GangUrlBuilder } from '../models';
import { IGangAuthSettings } from './IGangAuthSettings';

export class GangAuthService {

  constructor(
    settings: IGangAuthSettings = null,
    private window: {
      location: { href: string },
      document: { title: string },
      history: { pushState: (state: any, title: string, url?: string) => void }
    } = null
  ) {
    this.settings = {
      rootUrl: 'api/gang/auth',
      ...settings
    }
  }

  settings: IGangAuthSettings;

  /**
   * Checks the url for a link token
   *
   * if found, it will be used to fetch a session token
   *
   * if successful, the link token is removed from the url
   *
   * @param {string} [parameterName=link-token] - name of the url parameter
   */
  async tryLinkInUrl(
    parameterName: string = 'link-token'
  ): Promise<string> {

    let token: string = null;
    let win = this.window || window;

    const urlBuilder = new GangUrlBuilder(win.location.href);
    const linkToken = urlBuilder.parameters[parameterName];

    if (linkToken) {
      token = await this.getTokenFromLink(linkToken[0]);

      delete urlBuilder.parameters[parameterName];
      win.history.pushState(null, win.document.title, urlBuilder.build());
    }

    return token;
  }

  /**
   * Attempts to get a session token, given a link token
   *
   * @param {string} [linkToken] - link token
   */
  async getTokenFromLink(linkToken: string): Promise<string> {

    const result = await fetch(`${this.settings.rootUrl}/link/${linkToken}`);
    return result.ok
      ? await result.text()
      : null;
  }
}
