
export function unescapeText(value: string): string {
  if (!value)
    return value;

  try {
    return decodeURIComponent(escape(value));
  } catch {
    return value;
  }
}
