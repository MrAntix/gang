import { GangService, createGangWebSocket, GangAuthenticationService, GangHttp, GangLocationService, GangVault, } from './services';
/**
 * Provides static access to services
 */
export class GangContext {
    constructor() { }
    static get service() {
        return (GangContext._service ||
            (GangContext._service = new GangService(createGangWebSocket, GangContext.settings, GangContext.initialState)));
    }
    static get auth() {
        return (GangContext._auth ||
            (GangContext._auth = new GangAuthenticationService(GangContext.settings, new GangHttp(GangContext.settings.authRootPath), GangLocationService.instance, navigator.credentials)));
    }
    static get vault() {
        return GangContext._vault || (GangContext._vault = new GangVault(GangContext.settings.vault));
    }
}
GangContext.logger = () => undefined;
GangContext.defaultSettings = {
    app: {
        id: null,
        name: 'Gang App',
    },
    rootUrl: `${location.protocol.replace('http', 'ws')}//${location.host}`,
    authRootPath: '/api/gang/auth',
    vault: {
        name: 'Gang',
        store: 'Vault',
        key: '$key',
    },
};
GangContext.settings = GangContext.defaultSettings;
GangContext.initialState = undefined;
