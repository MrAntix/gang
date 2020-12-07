export interface IAppMessage {
  id: string;
  on?: string|Date;
  byId?: string;
  text: string;
  class?: string;
}
