import { IGangAuth } from '../authentication';
import { GangEventTypes } from './GangEventTypes';

export interface IGangAuthenticatedHostEvent {
  type: GangEventTypes.Host;
  auth: IGangAuth;
}
