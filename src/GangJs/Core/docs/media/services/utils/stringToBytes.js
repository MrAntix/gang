export function stringToBytes(value) {
    return Uint8Array.from(value, (c) => c.charCodeAt(0));
}
