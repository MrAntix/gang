/** wraps a command */
export class GangCommandWrapper<T> {
  constructor(
    /** command type */
    public readonly type: string,
    /** command */
    public readonly command: T,
    /** sequence number */
    public readonly sn: number = undefined,
    /** in reply to sequence number */
    public readonly rsn: number = undefined
  ) {}
}
