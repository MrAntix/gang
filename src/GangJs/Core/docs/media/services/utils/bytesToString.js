export function bytesToString(value) {
    return String.fromCharCode.apply(null, new Uint8Array(value));
}
