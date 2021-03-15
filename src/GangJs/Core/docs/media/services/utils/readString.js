export function readString(buffer, start = 0, length) {
    return String.fromCharCode.apply(null, new Uint8Array(length ? buffer.slice(start, start + length) : buffer.slice(start)));
}
