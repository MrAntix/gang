import { GangNotificationTypes } from './GangNotificationTypes';

export function getGangNotificationTypeDisplay(value: GangNotificationTypes): string {
  switch (value) {
    default:
      return null;
    case GangNotificationTypes.info:
      return 'info';
    case GangNotificationTypes.success:
      return 'success';
    case GangNotificationTypes.warning:
      return 'warning';
    case GangNotificationTypes.danger:
      return 'danger';
  }
}
