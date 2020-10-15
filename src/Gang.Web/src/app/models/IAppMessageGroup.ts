import { IAppMessage } from './IAppMessage';

export interface IAppMessageGroup {
  time: number;
  userId: string;
  items: IAppMessage[];
}
