import { IAppUser } from './IAppUser';


export function userIsOnline(user: IAppUser): boolean {

  return !!user.memberIds?.length;
}
