import { IGangLocationService } from '../models';

/**
 * Small wrapper for location functions
 */
export class GangLocationService implements IGangLocationService {
  get host(): string {
    return location.host;
  }
  get origin(): string {
    return `${location.protocol}//${location.host}`;
  }
  get href(): string {
    return location.href;
  }
  pushState(url: string): void {
    history.pushState(null, document.title, url);
  }

  private static _instance: GangLocationService;
  public static get instance(): GangLocationService {
    return GangLocationService._instance || (GangLocationService._instance = new GangLocationService());
  }
}
