export class GangStore {

  static get(
    name: string,
    getDefault: () => string = null
  ): string {
    if (!window.localStorage)
      throw new Error('GangStore incompatible');

    const result = window.localStorage.getItem(name);

    if (result == null) {
      if (!getDefault) return undefined;

      const value = getDefault();
      GangStore.set(name, value);
      return value;
    }

    return result;
  }

  static set(name: string, value: string): void {
    if (!window.localStorage)
      throw new Error('GangStore incompatible');

    window.localStorage.setItem(name, value);
  }

  static delete(name: string): void {
    window.localStorage.removeItem(name);
  }
}
