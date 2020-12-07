export function getRandomBytes(length = 16): ArrayBuffer {
  return globalThis.crypto.getRandomValues(new Uint8Array(length));
}
