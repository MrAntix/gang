import { GangCommandTypes } from './GangCommands';
import { IGangNotification } from './IGangNotification';

export interface IGangNotify {
  type: GangCommandTypes.notify;
  data: IGangNotification;
  rsn: number;
}
