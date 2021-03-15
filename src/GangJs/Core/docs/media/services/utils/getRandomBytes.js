export function getRandomBytes(length = 16) {
    return globalThis.crypto.getRandomValues(new Uint8Array(length));
}
