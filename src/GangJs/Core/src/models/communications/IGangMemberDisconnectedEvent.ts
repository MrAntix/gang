import { GangEventTypes } from './GangEventTypes';

export interface IGangMemberDisconnectedEvent {
  type: GangEventTypes.MemberDisconnected;
  memberId: string;
}
