import { GangNotificationTypes } from "./GangNotificationTypes";


export interface IGangNotification {
  id: string;
  text: string;
  type: GangNotificationTypes;
  timeout: number;
}
