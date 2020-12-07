import { IAppMessage } from '../models';

export function sortMessages(items: IAppMessage[]): IAppMessage[] {
  const sorted = [...items];

  sorted.sort((a, b) => new Date(a.on).getTime() - new Date(b.on).getTime());
  return sorted;
}


