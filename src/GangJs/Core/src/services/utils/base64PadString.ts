export function base64PadString(value: string): string {
  return value + '==='.slice((value.length + 3) % 4);
}
