import { GangEventTypes } from './GangEventTypes';

export interface IGangCommandReceiptEvent {
  type: GangEventTypes.CommandReceipt;
  rsn: number;
}
