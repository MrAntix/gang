export function readString(buffer: ArrayBuffer, start = 0, length?: number): string {
  return String.fromCharCode.apply(
    null,
    new Uint8Array(length ? buffer.slice(start, start + length) : buffer.slice(start))
  );
}
