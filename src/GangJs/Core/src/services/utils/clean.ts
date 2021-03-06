export function clean<T>(obj: T, test: (v: unknown) => boolean = (v) => v == null): T {
  if (obj == null) return obj;

  return Object.entries(obj).reduce((n, [k, v]) => (test(v) ? n : ((n[k] = v), n)), {}) as T;
}
