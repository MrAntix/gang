import { IAppMessage, IAppMessageGroup } from '../models';
import { sortMessages } from './sortMessages';

export function sortAndGroupMessages(items: IAppMessage[]): IAppMessageGroup[] {

  return sortMessages(items)
    .reduce<IAppMessageGroup[]>((groups, item) => {
      const time = new Date(item.on).getTime();
      let group = groups[groups.length - 1];

      if (!group
        || group.byId !== item.byId
        || Math.abs(group.time - time) > 30000) {

        groups = [...groups, {
          time,
          byId: item.byId,
          items: [item]
        }];

      } else {

        groups = groups.map(
          g => g === group
            ? {
              ...group,
              time,
              items: [...group.items, item]
            }
            : g);

      }

      return groups;
    }, []);
}
