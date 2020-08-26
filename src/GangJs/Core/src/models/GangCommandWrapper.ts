import { getGangId } from '../services/getGangId';

export class GangCommandWrapper {
  constructor(public readonly type: string, public readonly command: unknown) {
    this.id = getGangId();
    this.on = new Date();
  }

  readonly id: string;
  readonly on: Date;
}
