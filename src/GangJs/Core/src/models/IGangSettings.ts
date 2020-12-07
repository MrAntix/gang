import { IGangApplication } from './IGangApplication';
import { IGangVaultSettings } from './storage';

export interface IGangSettings {
  app: IGangApplication;
  rootUrl: string;
  authRootPath: string;
  vault: IGangVaultSettings;
}
