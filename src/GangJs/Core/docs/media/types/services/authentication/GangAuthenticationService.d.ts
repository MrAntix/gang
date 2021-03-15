import { IGangHttp, IGangLocationService, IGangCredentialsService, IGangTokenData, IGangSettings, IGangPlatform, GangAuthenticationCredential } from '../../models';
export declare class GangAuthenticationService {
    private settings;
    private http;
    private location;
    private credentials;
    /**
     * Create a service
     * @param settings
     * @param http
     * @param location
     * @param credentials
     * @param vault
     */
    constructor(settings: IGangSettings, http: IGangHttp, location: IGangLocationService, credentials: IGangCredentialsService);
    private _platform;
    get platform(): IGangPlatform;
    /**
     * Request a link code
     *
     * @param email email address
     */
    requestLink(email: string): Promise<boolean>;
    /**
     * gets the code from the current url and removes if found
     *
     * @param {string} [parameterName=link-code] - name of the url parameter
     */
    tryGetLinkCodeFromUrl(parameterName?: string): string;
    /**
     * Attempts to get a session token, given a code
     *
     * @param {string} [code]
     */
    validateLink(email: string, code: string): Promise<string>;
    /**
     * Try and get a challenge for the user from the server
     *
     * @param token valid session token
     */
    tryGetChallenge(token?: string): Promise<ArrayBuffer>;
    /**
     * Register a credential from the device on the server
     * shows the authenticator UI e.g. fingerprint, face or pin
     *
     * @param token valid session token
     * @param challenge required challenge from the server
     *
     * @returns credential, which can be stored and passed to authenticateCredential
     */
    registerCredential(token: string, challenge: ArrayBuffer): Promise<GangAuthenticationCredential>;
    /**
     * Validate the credential passed
     * shows the authenticator UI e.g. fingerprint, face or pin
     *
     * throws on failure
     *
     * offline will only do basic validation
     * online will pass back to the server for detailed auth
     *
     * @param credential Stored registered credential
     *
     * @returns when offline returns null, online will return a new session token
     */
    validateCredential(credential: GangAuthenticationCredential): Promise<string>;
    private validate;
    /**
     * decode a token to data
     *
     * @param token valid token
     */
    decodeToken(token: string): IGangTokenData;
}
