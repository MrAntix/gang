import { GangService, createGangWebSocket, IGangAuthSettings } from './services';
import { GangLogger } from './GangLogger';
import { GangAuthService } from './services/GangAuthService';

export class GangContext {
  private static _service: GangService;
  private static _auth: GangAuthService;
  private constructor() {}

  public static logger: GangLogger = () => undefined;
  public static initialState: Record<string, unknown> = undefined;
  public static authSettings: IGangAuthSettings = undefined;

  public static get service(): GangService {
    return (
      GangContext._service || (GangContext._service = new GangService(createGangWebSocket, GangContext.initialState))
    );
  }

  public static get auth(): GangAuthService {
    return GangContext._auth || (GangContext._auth = new GangAuthService(GangContext.authSettings));
  }
}
