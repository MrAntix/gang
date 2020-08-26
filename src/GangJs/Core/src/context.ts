import { GangService, createGangWebSocket } from './services';
import { GangLogger } from './GangLogger';

export class GangContext {
  private static _service: GangService;
  private constructor() {}

  public static logger: GangLogger = () => undefined;

  public static get service(): GangService {
    return (
      GangContext._service ||
      (GangContext._service = new GangService(createGangWebSocket))
    );
  }
}
