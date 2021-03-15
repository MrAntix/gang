export function base64PadString(value) {
    return value + '==='.slice((value.length + 3) % 4);
}
