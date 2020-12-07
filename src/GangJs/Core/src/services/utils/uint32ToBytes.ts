export function uint32ToBytes(value: number): ArrayBuffer {
  const buffer = new ArrayBuffer(4);
  new DataView(buffer).setUint32(0, value, true);
  return buffer;
}
