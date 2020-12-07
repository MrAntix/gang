
export function escapeText(value: string): string {
  if (!value)
    return value;

  return unescape(encodeURIComponent(value));
}
