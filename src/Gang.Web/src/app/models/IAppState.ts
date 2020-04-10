import { IAppUser } from "./IAppUser";
import { IAppMessage } from './IAppMessage';

export interface IAppState {
  users: IAppUser[];
  messages: IAppMessage[];
}
