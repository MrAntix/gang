import { GangContext } from '../../context';
import { GangCredentialRegistration, GangAuthentication, GangAuthenticationCredential, } from '../../models';
import { GangUrlBuilder, base64UrlToBytes, bytesToBase64Url, bytesToString, CBOR, getRandomBytes, stringToBytes, } from '../utils';
export class GangAuthenticationService {
    /**
     * Create a service
     * @param settings
     * @param http
     * @param location
     * @param credentials
     * @param vault
     */
    constructor(settings, http, location, credentials) {
        this.settings = settings;
        this.http = http;
        this.location = location;
        this.credentials = credentials;
        this._platform = {
            hasAuthenticator: true,
        };
        if (window.PublicKeyCredential)
            PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable().then((value) => {
                this._platform = Object.assign(Object.assign({}, this.platform), { hasAuthenticator: value });
            });
    }
    get platform() {
        return this._platform;
    }
    /**
     * Request a link code
     *
     * @param email email address
     */
    async requestLink(email) {
        const result = await this.http.fetch(`/request-link`, {
            method: 'POST',
            headers: {
                'Content-type': 'application/json',
            },
            body: `"${email}"`,
        });
        return result.ok;
    }
    /**
     * gets the code from the current url and removes if found
     *
     * @param {string} [parameterName=link-code] - name of the url parameter
     */
    tryGetLinkCodeFromUrl(parameterName = 'link-code') {
        const urlBuilder = new GangUrlBuilder(this.location.href);
        const code = urlBuilder.getString(parameterName);
        if (code) {
            // code found, remove from the url and link
            delete urlBuilder.parameters[parameterName];
            this.location.pushState(urlBuilder.build());
            return code;
        }
        return undefined;
    }
    /**
     * Attempts to get a session token, given a code
     *
     * @param {string} [code]
     */
    async validateLink(email, code) {
        const result = await this.http.fetch(`/validate-link`, {
            method: 'POST',
            headers: {
                'Content-type': 'application/json',
            },
            body: JSON.stringify({
                email,
                code,
            }),
        });
        if (!result.ok)
            return undefined;
        return await result.text();
    }
    /**
     * Try and get a challenge for the user from the server
     *
     * @param token valid session token
     */
    async tryGetChallenge(token) {
        if (!token)
            return null;
        const headers = { 'Content-type': 'application/json' };
        if (token)
            headers['Authorization'] = token;
        const result = await this.http.fetch(`/request-challenge`, {
            method: 'POST',
            headers,
        });
        if (!result.ok)
            return null;
        return stringToBytes(await result.text());
    }
    /**
     * Register a credential from the device on the server
     * shows the authenticator UI e.g. fingerprint, face or pin
     *
     * @param token valid session token
     * @param challenge required challenge from the server
     *
     * @returns credential, which can be stored and passed to authenticateCredential
     */
    async registerCredential(token, challenge) {
        const tokenData = this.decodeToken(token);
        if (!tokenData)
            throw new Error('token is required');
        if (!challenge)
            throw new Error('challenge is required');
        const options = {
            challenge,
            rp: {
                name: this.settings.app.name,
            },
            user: {
                id: base64UrlToBytes(tokenData.id),
                name: tokenData.email,
                displayName: tokenData.name || tokenData.email,
            },
            pubKeyCredParams: [
                { alg: -7, type: 'public-key' },
                { alg: -257, type: 'public-key' }, // windows hello
            ],
            authenticatorSelection: {
                authenticatorAttachment: this.platform.hasAuthenticator ? 'platform' : 'cross-platform',
                userVerification: 'discouraged',
                requireResidentKey: true, // allows user data stored on key
            },
            attestation: 'none',
        };
        const credential = (await this.credentials.create({ publicKey: options }));
        const response = credential.response;
        this.validate(response.clientDataJSON, challenge);
        const attestationObject = CBOR.decode(response.attestationObject);
        const transports = response['getTransports'] && response['getTransports']();
        const credentialRegistration = GangCredentialRegistration.from(attestationObject.authData, transports, challenge);
        const result = await this.http.fetch(`/register-credential`, {
            method: 'POST',
            headers: {
                'Content-type': 'application/json',
                Authorization: token,
            },
            body: JSON.stringify(credentialRegistration),
        });
        if (!result.ok)
            throw new Error('Credential was not registered');
        return new GangAuthenticationCredential(credential.id, transports);
    }
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
    async validateCredential(credential) {
        GangContext.logger('GangAuthenticationService.validateCredential', { credential });
        const challenge = getRandomBytes();
        const options = {
            challenge,
            userVerification: 'required',
            allowCredentials: [
                {
                    id: base64UrlToBytes(credential.id),
                    type: 'public-key',
                    transports: credential.transports,
                },
            ],
        };
        let publicKey;
        try {
            publicKey = (await this.credentials.get({ publicKey: options }));
        }
        catch (err) {
            console.error(err);
            return null;
        }
        const response = publicKey.response;
        this.validate(response.clientDataJSON, challenge);
        const validation = GangAuthentication.from(credential.id, response.clientDataJSON, response.authenticatorData, response.signature);
        if (navigator.onLine) {
            const result = await this.http.fetch(`/validate-credential`, {
                method: 'POST',
                headers: {
                    'Content-type': 'application/json',
                },
                body: JSON.stringify(validation),
            });
            if (!result.ok)
                throw new Error('Credential Invalid');
            return await result.text();
        }
        return null; // doesn't return token when offline
    }
    validate(clientDataJSON, challenge) {
        const clientData = JSON.parse(bytesToString(clientDataJSON));
        const challengeString = bytesToBase64Url(challenge);
        if (clientData.challenge !== challengeString)
            throw new Error('Invalid authenticator response challenge');
        if (clientData.origin !== this.location.origin)
            throw new Error('Invalid authenticator response origin');
    }
    /**
     * decode a token to data
     *
     * @param token valid token
     */
    decodeToken(token) {
        if (!token)
            return undefined;
        const tokenParts = token.split('.');
        if (tokenParts.length != 2)
            return undefined;
        const data = tokenParts[0];
        return JSON.parse(atob(data));
    }
}
