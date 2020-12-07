export class GangStore {
  /**
   * get a value from the store
   * @param key
   *
   * @param getDefault callback when not found, value returned is stored
   */
  static get(key: string, getDefault: () => string = null): string {
    if (!window.localStorage) throw new Error('GangStore incompatible');

    const result = window.localStorage.getItem(key);

    if (result == null) {
      if (!getDefault) return undefined;

      const value = getDefault();
      GangStore.set(key, value);
      return value;
    }

    return result;
  }

  /**
   * set a value in the store
   *
   * @param key
   * @param value if undefined entry is deleted
   */
  static set(key: string, value?: string): void {
    if (!window.localStorage) throw new Error('GangStore incompatible');

    if (value === undefined) window.localStorage.removeItem(key);
    else window.localStorage.setItem(key, value);
  }
}
