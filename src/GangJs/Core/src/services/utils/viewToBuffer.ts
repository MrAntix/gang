export function viewToBuffer(view: DataView | Uint8Array | Uint16Array | Uint32Array): ArrayBuffer {
  return view.buffer.slice(view.byteOffset, view.byteOffset + view.byteLength);
}
