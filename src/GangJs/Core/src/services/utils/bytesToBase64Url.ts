import { bytesToString } from './bytesToString';

export function bytesToBase64Url(value: ArrayBuffer): string {
  return btoa(bytesToString(value)).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
}
