import { GangAuthenticationService } from './GangAuthenticationService';

describe('GangAuthenticationService', () => {

  const linkCode = 'ABC-DEF-GHI';
  const baseUrl = 'https://example.org';
  const token = 'TOKEN';

  it('requestLink', async () => {
    const emailAddress = 'test@local';
    const verify = { fetch: { url: null, init: { body: null } } };

    const service = getService({}, verify);
    const result = await service.requestLink(emailAddress);

    expect(verify.fetch.url).toBe(`/request-link`);
    expect(verify.fetch.init.body).toBe(`"${emailAddress}"`);
    expect(result).toBe(true);
  });

  it('tryGetTokenFromUrl with no code, returns undefined', async () => {

    const service = getService({ linkCode: null });
    const result = await service.tryGetTokenFromUrl();

    expect(result).toBe(undefined);
  });

  it('tryGetTokenFromUrl with code, removed code from location', async () => {

    const verify = { locationGo: null };
    const service = getService({ linkCode }, verify);
    await service.tryGetTokenFromUrl();

    expect(verify.locationGo).toBe(baseUrl);
  });

  it('tryGetTokenFromUrl with code, fetches new token', async () => {

    const verify = { fetch: { url: null } };
    const service = getService({ linkCode }, verify);
    await service.tryGetTokenFromUrl();

    expect(verify.fetch.url).toBe(`/link/${linkCode}`);
  });

  it('tryGetTokenFromUrl with code, returns true', async () => {

    const service = getService({ linkCode });
    const result = await service.tryGetTokenFromUrl();

    expect(result).toBe(token);
  });

  it('tryGetTokenFromUrl with code and parameter name', async () => {
    const linkCodeName = 'other-name';
    const verify = { fetch: { url: null } };

    const service = getService({ linkCode, linkCodeName }, verify);
    const result = await service.tryGetTokenFromUrl(linkCodeName);

    expect(verify.fetch.url).toBe(`/link/${linkCode}`);
    expect(result).toBe(token);
  });

  it('createCredential', async () => {

    const tokenData = {
      id: 'USER-ID',
      expires: new Date(Date.now() + 1000).toISOString(),
      name: 'USER-NAME',
      emailAddress: 'user@example.com',
      roles: []
    };
    const challenge = "CHALLENGE";

    const verify = { credentialsCreateOptions: null };

    const service = getService(undefined, verify);
    await service.createCredential(tokenData, challenge);

    expect(verify.credentialsCreateOptions).not.toBeNull();
  });

  function getService(options: {
    linkCode?: string,
    linkCodeName?: string
  } = null,
    verify: {
      fetch?: {
        url?: string,
        init?: RequestInit
      },
      locationGo?: string,
      credentialsCreateOptions?: CredentialCreationOptions
    } = {},
  ) {

    options = {
      linkCodeName: 'link-code',
      ...options
    };

    return new GangAuthenticationService(
      {
        fetch(url, init) {
          verify.fetch = { url, init };
          return Promise.resolve({ ok: true, text: () => Promise.resolve(token) });
        }
      },
      {
        href: `${baseUrl}${options.linkCode && `?${options.linkCodeName}=${options.linkCode}`}`,
        pushState: url => {
          verify.locationGo = url
        }
      },
      {
        create: async (options) => {

          verify.credentialsCreateOptions = options;

          return null;
        }
      }
    )
  }
});
