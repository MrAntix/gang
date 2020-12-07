import { IGangAuth } from '../authentication';
import { GangEventTypes } from './GangEventTypes';

export interface IGangAccessDeniedEvent {
  type: GangEventTypes.Denied;
  auth: IGangAuth;
}
