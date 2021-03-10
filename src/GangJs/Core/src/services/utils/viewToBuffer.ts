import { IView } from './IView';

export function viewToBuffer(view: IView): ArrayBuffer {
  return view.buffer.slice(view.byteOffset, view.byteOffset + view.byteLength);
}
