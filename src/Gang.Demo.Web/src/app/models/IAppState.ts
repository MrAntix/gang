import { IAppUser } from "./IAppUser";
import { IAppMessage } from './IAppMessage';

export interface IAppState extends Record<string, unknown> {
  users: IAppUser[];
  messages: IAppMessage[];
  notifications: Record<string, IAppMessage>;
  inviteSentTo: string
}
