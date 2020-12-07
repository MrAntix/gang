import { bytesToString } from './bytesToString';

export function bytesTo<T>(value: ArrayBuffer): T {
  return JSON.parse(bytesToString(value));
}
