import { IGangMemberConnectedEvent } from './IGangMemberConnectedEvent';
import { IGangAccessDeniedEvent } from './IGangAccessDeniedEvent';
import { IGangAuthenticatedMemberEvent } from './IGangAuthenticatedMemberEvent';
import { IGangAuthenticatedHostEvent } from './IGangAuthenticatedHostEvent';
import { IGangMemberDisconnectedEvent } from './IGangMemberDisconnectedEvent';
import { IGangCommandEvent } from './IGangCommandEvent';
import { IGangCommandReceiptEvent } from './IGangCommandReceiptEvent';
import { IGangStateEvent } from './IGangStateEvent';

export type GangEvents =
  | IGangAuthenticatedHostEvent
  | IGangAuthenticatedMemberEvent
  | IGangAccessDeniedEvent
  | IGangMemberConnectedEvent
  | IGangMemberDisconnectedEvent
  | IGangCommandEvent
  | IGangCommandReceiptEvent
  | IGangStateEvent;
