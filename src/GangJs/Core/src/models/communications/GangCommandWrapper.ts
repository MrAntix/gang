import { IGangCommand } from './IGangCommand';

/** wraps command data */
export class GangCommandWrapper<T> implements IGangCommand {
  constructor(
    /** command data type */
    public readonly type: string,
    /** command data */
    public readonly data: T,
    /** sequence number */
    public readonly sn: number = undefined,
    /** in reply to sequence number */
    public readonly rsn: number = undefined
  ) {}
}
