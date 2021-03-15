import { Observable } from 'rxjs';
import { GangConnectionState, GangCommandWrapper, GangWebSocketFactory, IGangCommandSent, IGangApplication, IGangSettings, IGangConnectionProperties } from '../models';
export declare class GangService<TState> {
    private readonly webSocketFactory;
    private readonly settings;
    private readonly initialState;
    private retry;
    private retrying;
    retryingIn: number;
    private webSocket;
    private connectionSubject;
    connectionProperties: IGangConnectionProperties;
    get connectionState(): GangConnectionState;
    get isConnected(): boolean;
    memberId: string;
    isAuthenticated: boolean;
    isHost: boolean;
    application: IGangApplication;
    private connectionRetrySubject;
    private authenticatedSubject;
    private memberConnectedSubject;
    private memberDisconnectedSubject;
    private commandSubject;
    private receiptSubject;
    private stateSubject;
    private unsentCommands;
    onConnection: Observable<GangConnectionState>;
    onConnectionRetry: Observable<number>;
    onAuthenticated: Observable<string>;
    onMemberConnected: Observable<string>;
    onMemberDisconnected: Observable<string>;
    onCommand: Observable<GangCommandWrapper<unknown>>;
    onReceipt: Observable<number>;
    onState: Observable<unknown>;
    constructor(webSocketFactory: GangWebSocketFactory, settings?: IGangSettings, initialState?: TState);
    /**
     * Connect to gang
     */
    connect(properties?: IGangConnectionProperties): Promise<boolean>;
    authenticate(token: string): void;
    disconnect(reason?: string): Promise<void>;
    private offline;
    private online;
    /** Set the local current state, ie not sent to the server
     *
     * @param state the passed state will be shallow merged with the current state
     */
    setState(state: Partial<TState>): void;
    /**
     * Map gang events to the component
     *
     * @param component the component to map to
     */
    mapEvents<TState>(component: {
        disconnectedCallback?: () => void;
        onGangConnection?: (connectionState: GangConnectionState) => void;
        onGangConnectionRetry?: (seconds: number) => void;
        onGangAuthenticated?: (token: string) => void;
        onGangState?: (state: TState) => void;
        onGangCommand?: (command: unknown) => void;
        onGangMemberConnected?: (memberId: string) => void;
        onGangMemberDisconnected?: (memberId: string) => void;
    }): void;
    /**
     * Map the actions to a component, injecting this service
     *
     * @param component the component to map to
     * @param actions a map of the executors e.g. { actionOne, actionTwo }
     */
    mapActions<C extends {
        [K in keyof A]: unknown;
    }, A>(component: C, actions: A): void;
    /**
     * Executes a command locally no data is sent to the host
     *
     * @param type Command type name
     * @param data Command data
     */
    executeCommand<T>(type: string, data: T): void;
    private sn;
    /**
     * Sends a command to the host member
     * await this if you expect a reply command from the host
     *
     * @param type Command type name
     * @param data Command data
     *
     * @returns a IGangCommandSent
     */
    sendCommand<T>(type: string, data: T): IGangCommandSent;
    private sendCommandWrapper;
    sendState(state: TState): void;
    private send;
    waitForCommand<T>(type: string, predicate: (c: T) => boolean, options?: {
        timeout?: number;
    }): Promise<void>;
    waitForState<T>(predicate: (s: T) => boolean, options?: {
        timeout?: number;
    }): Promise<void>;
    static setState(value: string): void;
}
