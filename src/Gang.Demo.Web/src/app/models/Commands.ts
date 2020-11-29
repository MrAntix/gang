
export type Commands = INotify;
export enum CommandTypes {
  notify = 'notify'
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
