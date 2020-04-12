export class GangStore {

  static get(
    name: string,
    getDefault: () => string = null
  ): string {
    if (typeof document === 'undefined')
      return undefined;

    name = encodeURIComponent(name);
    const regexp = new RegExp(`(?:^${name}|;\\s*${name})=(.*?)(?:;|$)`, 'g');
    const result = regexp.exec(document.cookie);
    if (!result || result.length === 0) {
      if (!getDefault) return undefined;

      const value = getDefault();
      GangStore.set(name, value);
      return value;
    }

    return decodeURIComponent(result[1]);
  }

  static set(name: string, value: string): void {
    let valueString: string = '';
    let expires = 60 * 24 * 7 * 5;

    if (value === null || value === undefined)
      expires = -1;
    else
      valueString = typeof value === 'string' ? value : JSON.stringify(value);

    let cookie = encodeURIComponent(name) + '=' + encodeURIComponent(valueString) + ';';

    const expiresDate = new Date(new Date().getTime() + expires * 1000 * 60);
    cookie += `expires=${expiresDate.toUTCString()};`;
    //cookie += 'secure;';

    document.cookie = cookie;
  }

  static delete(name: string): void {
    GangStore.set(name, null);
  }
}
