import { GangEventTypes } from './GangEventTypes';
export interface IGangStateEvent {
    type: GangEventTypes.State;
    state: Record<string, unknown>;
}
