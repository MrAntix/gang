import { GangLogger } from './GangLogger';
import { GangService, createGangWebSocket, GangAuthenticationService, GangHttp, GangLocationService } from './services';
import { IGangSettings } from './models';

/**
 * Provides static access to services
 */
export class GangContext {
  private static _service: GangService<Record<string, unknown>>;
  private static _auth: GangAuthenticationService;

  private constructor() { }

  public static logger: GangLogger = () => undefined;
  public static defaultSettings: IGangSettings = {
    rootUrl: `${location.protocol.replace('http', 'ws')}//${location.host}`,
    authRootPath: '/api/gang/auth'
  };
  public static settings: IGangSettings = GangContext.defaultSettings;
  public static initialState: Record<string, unknown> = undefined;

  public static get service(): GangService<Record<string, unknown>> {
    return (
      GangContext._service || (GangContext._service = new GangService(
        createGangWebSocket,
        GangContext.settings,
        GangContext.initialState))
    );
  }

  public static get auth(): GangAuthenticationService {
    return (
      GangContext._auth
      || (GangContext._auth = new GangAuthenticationService(
        new GangHttp(this.settings.authRootPath),
        GangLocationService.instance
      ))
    )
  }
}
