import { GangService } from '@gang-js/core';
import { getErrorMessage } from './getErrorMessage';
import { IAppState } from './IAppState';

export function provideSendCommand(services: {
  gang: GangService<IAppState>;
}) {

  return <T>(type: string, data: T, options?: {
    timeout?: number;
  }) => new Promise(async (resolve, reject) => {

    const result = await services.gang.sendCommand(type, data).wait(options);
    const error = result && getErrorMessage(result);

    if (!error)
      resolve(result);
    else
      reject(error);
  });
}
