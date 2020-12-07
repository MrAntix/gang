/* eslint-disable @typescript-eslint/no-explicit-any*/
export type GangData<T> = {
  [P in keyof T]: any;
};
