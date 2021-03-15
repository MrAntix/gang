import { IGangHttp, IGangHttpResponse } from '../models';
/**
 * Small wrapper for HTTP functions
 */
export declare class GangHttp implements IGangHttp {
    rootPath: string;
    constructor(rootPath: string);
    fetch(url: string, init: RequestInit): Promise<IGangHttpResponse>;
}
