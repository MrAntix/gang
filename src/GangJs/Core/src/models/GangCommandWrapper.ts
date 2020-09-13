export class GangCommandWrapper<T> {
  constructor(
    public readonly type: string,
    public readonly command: T,
    public readonly sn: number = undefined
  ) {}
}
