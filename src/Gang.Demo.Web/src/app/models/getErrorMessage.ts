import { GangCommandWrapper } from '@gang-js/core';
import { Commands, CommandTypes } from './Commands';

export function getErrorMessage(
  command: GangCommandWrapper<unknown>): string {

  const c = command as Commands;

  switch (c.type) {
    default: return null;
    case CommandTypes.notify:
      return c.data.message;
  }
}
