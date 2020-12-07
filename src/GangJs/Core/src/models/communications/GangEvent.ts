import { GangEvents } from './GangEvents';
import { GangEventTypes } from './GangEventTypes';

export type GangEvent<T extends GangEventTypes> = Extract<GangEvents, { type: T }>;
