export type Commands = ISetSettings;
export enum CommandTypes {
  setSettings = 'setSettings'
}

export interface ISetSettings {
  type: CommandTypes.setSettings;
  data: {
    authEnabled: boolean;
  };
}
