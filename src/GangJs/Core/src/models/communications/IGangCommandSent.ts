import { GangCommandWrapper } from './GangCommandWrapper';
import { IGangCommandWaitOptions } from './IGangCommandWaitOptions';

/** Send command response */
export interface IGangCommandSent {
  /** assigned sequence number */
  sn: number;
  /** promise resolves on matching rsn (receipt sequence number) from server,
   * should be checked as can be undefined when host or offline
   */
  wait?: (options?: IGangCommandWaitOptions) => Promise<GangCommandWrapper<unknown>>;
}
