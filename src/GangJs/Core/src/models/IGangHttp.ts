import { IGangHttpResponse } from './IGangHttpResponse';

/**
 * interface for http methods
 */
export interface IGangHttp {
  fetch(url: string, init?: RequestInit): Promise<IGangHttpResponse>;
}
