import { GangService, createGangWebSocket } from './services';

export class GangContext {

  private static _service: GangService;
  private constructor() { }

  public static get service() {
    return this._service
      || (this._service = new GangService(createGangWebSocket));
  }
}
