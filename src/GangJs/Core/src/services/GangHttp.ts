import { IGangHttp, IGangHttpResponse } from '../models';

/**
 * Small wrapper for HTTP functions
 */
export class GangHttp
  implements IGangHttp {

  constructor(
    public rootPath: string) {
  }

  fetch(url: string, init: RequestInit): Promise<IGangHttpResponse> {

    return fetch(`${this.rootPath}${url}`, init);
  }
}
