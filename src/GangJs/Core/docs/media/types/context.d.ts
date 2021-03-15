import { GangLogger } from './GangLogger';
import { GangService, GangAuthenticationService, GangVault } from './services';
import { IGangSettings } from './models';
/**
 * Provides static access to services
 */
export declare class GangContext {
    private static _service;
    private static _auth;
    private static _vault;
    private constructor();
    static logger: GangLogger;
    static defaultSettings: IGangSettings;
    static settings: IGangSettings;
    static initialState: Record<string, unknown>;
    static get service(): GangService<Record<string, unknown>>;
    static get auth(): GangAuthenticationService;
    static get vault(): GangVault;
}
