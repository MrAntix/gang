export function stringToBytes(value: string): ArrayBuffer {
  return Uint8Array.from(value, (c) => c.charCodeAt(0));
}
