
export function arrayToMap<T>(array: T[], getKey: (i: T) => string): Record<string, T> {

  return array.reduce((m, i) => {
    m[getKey(i)] = i;

    return m;
  }, {});

}
