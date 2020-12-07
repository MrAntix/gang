import { IGangLocationService } from "../models";

/**
 * Small wrapper for location functions
 */
export class GangLocationService
  implements IGangLocationService {

  get href(): string { return window.location.href; }
  pushState(url: string): void {

    window.history.pushState(null, document.title, url);
  }

  private static _instance: GangLocationService;
  public static get instance(): GangLocationService {
    return GangLocationService._instance || (
      GangLocationService._instance = new GangLocationService()
    );
  }
}
