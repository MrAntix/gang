import { GangCommands, GangCommandTypes, GangCommandWrapper } from '@gang-js/core';
import { Commands } from './Commands';

export function getErrorMessage(
  command: GangCommandWrapper<unknown>): string {

  const c = command as Commands | GangCommands;

  switch (c.type) {
    default: return null;
    case GangCommandTypes.notify:
      return c.data.text;
  }
}
