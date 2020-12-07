import { IGangApplication } from '../IGangApplication';

export interface IGangAuth {
  memberId: string;
  token: string;
  application: IGangApplication;
}
