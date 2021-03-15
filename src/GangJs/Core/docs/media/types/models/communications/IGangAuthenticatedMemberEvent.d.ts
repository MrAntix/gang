import { IGangAuth } from '../authentication';
import { GangEventTypes } from './GangEventTypes';
export interface IGangAuthenticatedMemberEvent {
    type: GangEventTypes.Member;
    auth: IGangAuth;
}
