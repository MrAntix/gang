import { GangCommandWrapper } from './GangCommandWrapper';
import { GangEventTypes } from './GangEventTypes';

export interface IGangCommandEvent {
  type: GangEventTypes.Command;
  wrapper: GangCommandWrapper<unknown>;
}
