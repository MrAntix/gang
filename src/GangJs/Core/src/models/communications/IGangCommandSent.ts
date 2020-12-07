import { GangCommandWrapper } from './GangCommandWrapper';
import { IGangCommandWaitOptions } from './IGangCommandWaitOptions';

export interface IGangCommandSent {
  sn: number;
  wait: (options?: IGangCommandWaitOptions) => Promise<GangCommandWrapper<unknown>>;
}
