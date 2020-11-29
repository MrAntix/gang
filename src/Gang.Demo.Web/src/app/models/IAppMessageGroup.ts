import { IAppMessage } from './IAppMessage';

export interface IAppMessageGroup {
  time: number;
  byId: string;
  items: IAppMessage[];
}
