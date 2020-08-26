import { GangWebSocket } from './GangWebSocket';

export type GangWebSocketFactory = (
  url: string,
  onOpen: (e: Event) => void,
  onError: (e: Event) => void,
  onClose: (e: CloseEvent) => void
) => GangWebSocket;
