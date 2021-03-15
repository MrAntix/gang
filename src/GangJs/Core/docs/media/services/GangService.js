import { BehaviorSubject, Subject, ReplaySubject } from 'rxjs';
import { GangContext } from '../context';
import { GangConnectionState, GangCommandWrapper, NO_RETRY, RETRY_INIT, RETRY_MAX, GangEventTypes, } from '../models';
import { GangStore } from './storage';
import { GangUrlBuilder, clean } from './utils';
const GANG_AUTHENTICATION_TOKEN = 'GANG.AUTHENTICATION.TOKEN';
const GANG_STATE = 'GANG.STATE';
export class GangService {
    constructor(webSocketFactory, settings = GangContext.defaultSettings, initialState = null) {
        this.webSocketFactory = webSocketFactory;
        this.settings = settings;
        this.initialState = initialState;
        this.retry = RETRY_INIT;
        this.retryingIn = undefined;
        this.unsentCommands = [];
        this.sn = 0;
        this.onConnection = this.connectionSubject = new BehaviorSubject(GangConnectionState.disconnected);
        this.onConnectionRetry = this.connectionRetrySubject = new Subject();
        this.onAuthenticated = this.authenticatedSubject = new ReplaySubject(1);
        this.onMemberConnected = this.memberConnectedSubject = new Subject();
        this.onMemberDisconnected = this.memberDisconnectedSubject = new Subject();
        this.onCommand = this.commandSubject = new Subject();
        this.onReceipt = this.receiptSubject = new Subject();
        this.onState = this.stateSubject = new BehaviorSubject(initialState);
    }
    get connectionState() {
        return this.connectionSubject.value;
    }
    get isConnected() {
        return this.connectionSubject.value === GangConnectionState.connected;
    }
    /**
     * Connect to gang
     */
    async connect(properties) {
        if (this.isConnected)
            await this.disconnect('reconnect');
        this.connectionProperties = Object.assign(Object.assign(Object.assign({}, this.connectionProperties), { token: GangStore.get(GANG_AUTHENTICATION_TOKEN) }), clean(properties));
        GangContext.logger('GangService.connect', this.connectionProperties);
        return new Promise((resolve) => {
            this.connectionSubject.next(GangConnectionState.connecting);
            const connectUrl = GangUrlBuilder.from(this.settings.rootUrl + this.connectionProperties.path)
                .set('gangId', this.connectionProperties.gangId)
                .set('token', this.connectionProperties.token)
                .build();
            GangContext.logger('GangService.connected', this.settings.rootUrl + this.connectionProperties.path, connectUrl);
            this.webSocket = this.webSocketFactory(connectUrl, (e) => {
                GangContext.logger('GangService.onopen', e);
                this.connectionSubject.next(GangConnectionState.connected);
                this.retry = RETRY_INIT;
                this.retryingIn = undefined;
                resolve(true);
                let wrapper;
                while ((wrapper = this.unsentCommands.shift()))
                    this.sendCommandWrapper(wrapper);
                window.removeEventListener('online', this.online);
                this.offline = () => {
                    GangContext.logger('GangService.offline');
                    this.isConnected && this.disconnect(null);
                };
                window.addEventListener('offline', this.offline);
                clearRetryConnect();
            }, (e) => {
                GangContext.logger('GangService.onerror', e);
                this.connectionSubject.next(GangConnectionState.error);
                resolve(false);
            }, (e) => {
                GangContext.logger('GangService.onclose', e);
                this.connectionSubject.next(GangConnectionState.disconnected);
                window.removeEventListener('offline', this.offline);
                if (!e.reason) {
                    this.online = () => {
                        GangContext.logger('GangService.online');
                        !this.isConnected &&
                            this.connect(properties).catch(() => {
                                // do nothing.
                            });
                    };
                    window.addEventListener('online', this.online);
                    retryConnect();
                }
            });
            this.webSocket.subscribe((e) => {
                GangContext.logger('GangService.webSocket.message', e);
                switch (e.type) {
                    case GangEventTypes.Host:
                        this.isHost = true;
                        this.memberId = e.auth.memberId;
                        this.application = e.auth.application;
                        this.authenticate(e.auth.token);
                        break;
                    case GangEventTypes.Member:
                        this.isHost = false;
                        this.memberId = e.auth.memberId;
                        this.application = e.auth.application;
                        this.authenticate(e.auth.token);
                        break;
                    case GangEventTypes.Denied:
                        this.memberId = e.auth.memberId;
                        this.application = e.auth.application;
                        this.authenticate(e.auth.token);
                        break;
                    case GangEventTypes.MemberConnected:
                        this.memberConnectedSubject.next(e.memberId);
                        break;
                    case GangEventTypes.MemberDisconnected:
                        this.memberDisconnectedSubject.next(e.memberId);
                        break;
                    case GangEventTypes.Command:
                        this.commandSubject.next(e.wrapper);
                        break;
                    case GangEventTypes.CommandReceipt:
                        this.receiptSubject.next(e.rsn);
                        this.unsentCommands = this.unsentCommands.filter((w) => w.sn !== e.rsn);
                        break;
                    case GangEventTypes.State:
                        this.stateSubject.next(Object.assign(Object.assign({}, this.stateSubject.value), e.state));
                        break;
                }
            });
            const retryConnect = (() => {
                if (this.retry === NO_RETRY || this.retrying || this.isConnected)
                    return;
                GangContext.logger('GangService.retryConnect in', this.retry);
                this.retryingIn = this.retry;
                this.retrying = window.setInterval(() => {
                    this.retryingIn--;
                    this.connectionRetrySubject.next(this.retryingIn);
                    if (this.retryingIn === 0) {
                        clearRetryConnect();
                        this.connect(properties).catch(() => {
                            // do nothing.
                        });
                    }
                }, 1000);
                if (this.retry < RETRY_MAX)
                    this.retry *= 2;
            }).bind(this);
            const clearRetryConnect = (() => {
                if (this.retrying) {
                    clearInterval(this.retrying);
                    this.retrying = null;
                }
            }).bind(this);
            retryConnect();
        });
    }
    authenticate(token) {
        GangStore.set(GANG_AUTHENTICATION_TOKEN, token);
        this.isAuthenticated = !!token;
        this.authenticatedSubject.next(token);
        let state = this.initialState;
        if (this.isAuthenticated) {
            const stateJson = GangStore.get(GANG_STATE);
            if (stateJson)
                state = JSON.parse(stateJson);
        }
        this.stateSubject.next(state);
    }
    disconnect(reason = 'disconnected') {
        GangContext.logger('GangService.disconnect');
        return new Promise((resolve) => {
            if (this.webSocket) {
                let wait = null;
                wait = this.connectionSubject.subscribe((state) => {
                    if (state === GangConnectionState.disconnected) {
                        GangContext.logger('GangService.disconnect disconnected');
                        wait === null || wait === void 0 ? void 0 : wait.unsubscribe();
                        resolve();
                    }
                });
                this.retry = NO_RETRY;
                this.webSocket.close(reason);
            }
            else {
                resolve();
            }
        });
    }
    /** Set the local current state, ie not sent to the server
     *
     * @param state the passed state will be shallow merged with the current state
     */
    setState(state) {
        GangStore.set(GANG_STATE, JSON.stringify(state));
        this.stateSubject.next(Object.assign(Object.assign({}, this.stateSubject.value), state));
    }
    /**
     * Map gang events to the component
     *
     * @param component the component to map to
     */
    mapEvents(component) {
        const subs = [];
        [
            'Connection',
            'Authenticated',
            'State',
            'Command',
            'MemberConnected',
            'MemberDisconnected',
            'ConnectionRetry',
        ].forEach((key) => {
            const componentKey = `onGang${key}`;
            const serviceKey = `on${key}`;
            if (component[componentKey])
                subs.push(this[serviceKey].subscribe((e) => component[componentKey](e)));
            else if (component[serviceKey] !== undefined)
                console.warn(`${serviceKey} changed to ${componentKey}, please update your code`, component);
        });
        const disconnectedCallback = component.disconnectedCallback;
        component.disconnectedCallback = () => {
            if (disconnectedCallback)
                disconnectedCallback();
            subs.forEach((sub) => sub.unsubscribe());
        };
    }
    /**
     * Map the actions to a component, injecting this service
     *
     * @param component the component to map to
     * @param actions a map of the executors e.g. { actionOne, actionTwo }
     */
    mapActions(component, actions) {
        Object.keys(actions).forEach((key) => {
            component[key] = actions[key](this);
        });
    }
    /**
     * Executes a command locally no data is sent to the host
     *
     * @param type Command type name
     * @param data Command data
     */
    executeCommand(type, data) {
        const wrapper = new GangCommandWrapper(type, data);
        GangContext.logger('GangService.executeCommand', {
            wrapper,
            isConnected: this.isConnected,
        });
        this.commandSubject.next(wrapper);
    }
    /**
     * Sends a command to the host member
     * await this if you expect a reply command from the host
     *
     * @param type Command type name
     * @param data Command data
     *
     * @returns a IGangCommandSent
     */
    sendCommand(type, data) {
        const sn = ++this.sn;
        const wrapper = new GangCommandWrapper(type, data, sn);
        GangContext.logger('GangService.sendCommand', {
            wrapper,
            isConnected: this.isConnected,
        });
        this.unsentCommands = [...this.unsentCommands, wrapper];
        if (!this.isConnected)
            return;
        if (this.isHost) {
            this.commandSubject.next(wrapper);
            return;
        }
        return this.sendCommandWrapper(wrapper);
    }
    sendCommandWrapper(wrapper) {
        this.send(JSON.stringify({
            type: wrapper.type,
            data: wrapper.data,
        }), wrapper.sn);
        GangContext.logger('GangService.sendCommandWrapper', wrapper);
        return {
            sn: wrapper.sn,
            wait: (options) => new Promise((resolve) => {
                const sub = this.onCommand.subscribe((w) => {
                    if (w.rsn == wrapper.sn) {
                        sub.unsubscribe();
                        resolve(w);
                    }
                });
                setTimeout(() => {
                    sub.unsubscribe();
                    resolve(null);
                }, (options === null || options === void 0 ? void 0 : options.timeout) || 30000);
            }),
        };
    }
    sendState(state) {
        if (!this.isHost)
            throw new Error('only host can send state');
        this.stateSubject.next(state);
        this.send(JSON.stringify(state));
        GangContext.logger('GangService.sendState', state);
    }
    send(data, sn) {
        let a = Uint8Array.from(data, (x) => x.charCodeAt(0));
        if (sn) {
            const sna = Uint32Array.from([sn]);
            a = new Uint8Array([...new Uint8Array(sna.buffer), ...a]);
        }
        try {
            this.webSocket.send(a.buffer);
        }
        catch (err) {
            GangContext.logger('GangService.send error', err);
        }
    }
    waitForCommand(type, predicate, options) {
        const test = (c) => {
            return (!type || type === c.type) && (!predicate || predicate(c.data));
        };
        return new Promise((resolve, reject) => {
            const sub = this.onCommand.subscribe((c) => {
                if (test(c)) {
                    sub.unsubscribe();
                    resolve();
                }
            });
            setTimeout(() => {
                sub.unsubscribe();
                reject();
            }, (options === null || options === void 0 ? void 0 : options.timeout) || 10000);
        });
    }
    waitForState(predicate, options) {
        return new Promise((resolve, reject) => {
            const sub = this.onState.subscribe((s) => {
                if (predicate(s)) {
                    sub.unsubscribe();
                    resolve();
                }
            });
            setTimeout(() => {
                sub.unsubscribe();
                reject();
            }, (options === null || options === void 0 ? void 0 : options.timeout) || 10000);
        });
    }
    static setState(value) {
        GangStore.set(GANG_STATE, value);
    }
}
