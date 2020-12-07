export function setBytes(buffer: ArrayBuffer, offset: number, value: ArrayBuffer): void {
  new Uint8Array(buffer).set(new Uint8Array(value), offset);
}
