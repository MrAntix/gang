
export function mapToArray<T>(map: Record<string, T>) {
  if (map == null)
    return null;

  return Object.keys(map).map(k => map[k]).filter(v => v != null);
}
