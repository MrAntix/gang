import { bytesToString } from './bytesToString';
export function bytesTo(value) {
    return JSON.parse(bytesToString(value));
}
