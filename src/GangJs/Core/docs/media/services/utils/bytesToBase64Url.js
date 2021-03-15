import { bytesToString } from './bytesToString';
export function bytesToBase64Url(value) {
    return btoa(bytesToString(value)).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
}
