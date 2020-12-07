export function readUint32(buffer: ArrayBuffer, start = 0): number {
  return new Uint32Array(buffer.slice(start, start + 4))[0];
}
