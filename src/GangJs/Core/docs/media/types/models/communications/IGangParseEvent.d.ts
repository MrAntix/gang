import { GangEvents } from './GangEvents';
export interface IGangParseEvent {
    (data: ArrayBuffer): GangEvents;
}
