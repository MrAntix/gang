export function bytesToString(value: ArrayBuffer): string {
  return String.fromCharCode.apply(null, new Uint8Array(value));
}
