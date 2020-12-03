
export type Commands = INotify | ISetSettings;
export enum CommandTypes {
  notify = 'notify',
  setSettings = 'setSettings'
}

export interface INotify {
  type: CommandTypes.notify;
  data: {
    id: string;
    type: string;
    message: string;
  };
  rsn: number;
}

export interface ISetSettings {
  type: CommandTypes.setSettings;
  data: {
    authEnabled: boolean;
  };
}
