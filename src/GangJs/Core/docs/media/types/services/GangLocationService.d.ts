import { IGangLocationService } from '../models';
/**
 * Small wrapper for location functions
 */
export declare class GangLocationService implements IGangLocationService {
    get host(): string;
    get origin(): string;
    get href(): string;
    pushState(url: string): void;
    private static _instance;
    static get instance(): GangLocationService;
}
