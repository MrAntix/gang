import { stringToBytes } from './stringToBytes';

export function toBytes<T>(value: T): ArrayBuffer {
  return stringToBytes(JSON.stringify(value));
}
