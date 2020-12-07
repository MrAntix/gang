import { IAppUser } from '../models';


export function replaceUserIds(
  text: string,
  users: IAppUser[]
): string {
  if (!text)
    return;

  return users.reduce((text, user) => {

    return text.replace(`@${user.id}`, user.name);

  }, text);
}
