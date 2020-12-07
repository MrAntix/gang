import { GangLogger } from './GangLogger';
import {
  GangService,
  createGangWebSocket,
  GangAuthenticationService,
  GangHttp,
  GangLocationService,
  GangVault,
} from './services';
import { IGangSettings } from './models';

/**
 * Provides static access to services
 */
export class GangContext {
  private static _service: GangService<Record<string, unknown>>;
  private static _auth: GangAuthenticationService;
  private static _vault: GangVault;

  private constructor() {}

  public static logger: GangLogger = () => undefined;
  public static defaultSettings: IGangSettings = {
    app: {
      id: null,
      name: 'Gang App',
    },
    rootUrl: `${location.protocol.replace('http', 'ws')}//${location.host}`,
    authRootPath: '/api/gang/auth',
    vault: {
      name: 'Gang',
      store: 'Vault',
      key: '$key',
    },
  };
  public static settings: IGangSettings = GangContext.defaultSettings;
  public static initialState: Record<string, unknown> = undefined;

  public static get service(): GangService<Record<string, unknown>> {
    return (
      GangContext._service ||
      (GangContext._service = new GangService(createGangWebSocket, GangContext.settings, GangContext.initialState))
    );
  }

  public static get auth(): GangAuthenticationService {
    return (
      GangContext._auth ||
      (GangContext._auth = new GangAuthenticationService(
        GangContext.settings,
        new GangHttp(GangContext.settings.authRootPath),
        GangLocationService.instance,
        navigator.credentials
      ))
    );
  }

  public static get vault(): GangVault {
    return GangContext._vault || (GangContext._vault = new GangVault(GangContext.settings.vault));
  }
}
