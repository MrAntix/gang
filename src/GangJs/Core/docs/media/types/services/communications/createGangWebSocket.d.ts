import { GangWebSocket, IWebSocket } from '../../models';
export declare function createGangWebSocket(url: string, onOpen: (e: Event) => void, onError: (e: Event) => void, onClose: (e: CloseEvent) => void, create?: (url: string) => IWebSocket): GangWebSocket;
