import { IAppUser } from '../models';


export function sortUsers(items: IAppUser[]): IAppUser[] {
  const sorted = [...items];

  sorted.sort((a, b) => a.isOnline && b.isOnline
    ? a.name?.localeCompare(b.name)
    : a.isOnline ? -1 : 1);
  return sorted;
}
