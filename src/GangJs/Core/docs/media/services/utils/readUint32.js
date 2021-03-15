export function readUint32(buffer, start = 0) {
    return new Uint32Array(buffer.slice(start, start + 4))[0];
}
