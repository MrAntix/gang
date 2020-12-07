import { IGangVault } from '../../models';
import { GangAuthenticationService } from './GangAuthenticationService';

describe('GangAuthenticationService', () => {
  const linkCode = 'ABC-DEF-GHI';
  const host = 'example.org';
  const origin = 'https://example.org';
  const token = 'TOKEN';

  it('requestLink', async () => {
    const email = 'test@local';
    const verify = { fetch: { url: null, init: { body: null } } };

    const service = getService({}, verify);
    const result = await service.requestLink(email);

    expect(verify.fetch.url).toBe(`/request-link`);
    expect(verify.fetch.init.body).toBe(`"${email}"`);
    expect(result).toBe(true);
  });

  it('tryAuthorizeLinkFromUrl with no code, returns undefined', async () => {
    const service = getService({ linkCode: null });
    const result = await service.tryGetLinkCodeFromUrl();

    expect(result).toBe(undefined);
  });

  it('tryAuthorizeLinkFromUrl with code, returns code', async () => {
    const verify = { fetch: { url: null } };
    const service = getService({ linkCode }, verify);
    const result = await service.tryGetLinkCodeFromUrl();

    expect(result).toBe(linkCode);
  });

  it('tryAuthorizeLinkFromUrl with code, removed code from location', async () => {
    const verify = { locationGo: null };
    const service = getService({ linkCode }, verify);
    await service.tryGetLinkCodeFromUrl();

    expect(verify.locationGo).toBe(origin);
  });

  // it('registerCredential', async () => {

  //   const challenge = stringToBytes("CHALLENGE");

  //   const verify = { credentialsCreateOptions: null };

  //   const service = getService(undefined, verify);
  //   await service.registerCredential(token, challenge);

  //   expect(verify.credentialsCreateOptions).not.toBeNull();
  // });

  function getService(
    options: {
      linkCode?: string;
      linkCodeName?: string;
    } = null,
    verify: {
      fetch?: {
        url?: string;
        init?: RequestInit;
      };
      locationGo?: string;
      credentialsCreateOptions?: CredentialCreationOptions;
      credentialsGetOptions?: CredentialRequestOptions;
    } = {}
  ) {
    options = {
      linkCodeName: 'link-code',
      ...options,
    };

    return new GangAuthenticationService(
      {
        app: {
          id: null,
          name: 'Gang App',
        },
        rootUrl: `wss://example.com`,
        authRootPath: '/api/gang/auth',
        vault: {
          name: 'Gang',
          store: 'Vault',
          key: '$key',
        },
      },
      {
        fetch(url, init) {
          verify.fetch = { url, init };
          return Promise.resolve({ ok: true, text: () => Promise.resolve(token) });
        },
      },
      {
        host,
        origin,
        href: `${origin}${options.linkCode && `?${options.linkCodeName}=${options.linkCode}`}`,
        pushState: (url) => {
          verify.locationGo = url;
        },
      },
      {
        create: async (options) => {
          verify.credentialsCreateOptions = options;

          return null;
        },
        get: async (options) => {
          verify.credentialsGetOptions = options;

          return null;
        },
      }
    );
  }
});

export class FakeVault implements IGangVault {
  constructor(
    private data: { [key: string]: unknown } = {},
    private encryptedData: { [key: string]: ArrayBuffer } = {}
  ) {}

  deleteCryptoKey(): Promise<void> {
    throw new Error('Method not implemented.');
  }
  getCryptoKey(): Promise<CryptoKey> {
    throw new Error('Method not implemented.');
  }
  setCryptoKey(): Promise<CryptoKey> {
    throw new Error('Method not implemented.');
  }
  get<T>(key: string, getDefault?: () => T): Promise<T> {
    return Promise.resolve((this.data[key] as T) || (getDefault && getDefault()));
  }
  getEncrypted(key: string): Promise<ArrayBuffer> {
    return Promise.resolve(this.encryptedData[key] as ArrayBuffer);
  }
  set<T>(key: string, value: T): Promise<void> {
    this.data[key] = value;
    return Promise.resolve();
  }
  setEncrypted(key: string, value: ArrayBuffer): Promise<void> {
    this.encryptedData[key] = value;
    return Promise.resolve();
  }
  delete(key: string): Promise<void> {
    delete this.data[key];
    return Promise.resolve();
  }
}
