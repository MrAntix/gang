import { GangContext } from '@gang-js/core';

export default async () => {
  /**
   * The code to be executed should be placed within a default function that is
   * exported by the global script. Ensure all of the code in the global script
   * is wrapped in the function() that is exported.
   */

  GangContext.logger = console.debug;
  GangContext.initialState = {
    users: [],
    messages: [],
    privateMessages: []
  }
};
