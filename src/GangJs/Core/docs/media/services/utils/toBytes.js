import { stringToBytes } from './stringToBytes';
export function toBytes(value) {
    return stringToBytes(JSON.stringify(value));
}
