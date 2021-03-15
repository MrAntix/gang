export function setBytes(buffer, offset, value) {
    new Uint8Array(buffer).set(new Uint8Array(value), offset);
}
