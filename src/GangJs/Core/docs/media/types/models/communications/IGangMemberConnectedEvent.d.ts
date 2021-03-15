import { GangEventTypes } from './GangEventTypes';
export interface IGangMemberConnectedEvent {
    type: GangEventTypes.MemberConnected;
    memberId: string;
}
