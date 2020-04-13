import { GangWebSocket } from './GangWebSocket';

export interface IGangWebSocketFactory {

  (url: string,
    onOpen: (e: Event) => void,
    onError: (e: Event) => void,
    onClose: (e: CloseEvent) => void): GangWebSocket;
}
