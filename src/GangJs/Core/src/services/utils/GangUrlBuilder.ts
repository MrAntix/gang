export class GangUrlBuilder {
  constructor(urlString: string) {
    const protocolSplit = urlString.split(':');
    if (protocolSplit.length < 2) throw new Error(`GangUrlBuilder.ctor protocol not found ${urlString}`);
    this.protocol = protocolSplit[0];

    const path = protocolSplit.slice(1).join(':').split('?');
    this.root = path[0];
    this.parameters = {};
    if (path[1]) {
      path[1].split('&').forEach((part) => {
        const kv = part.split('=');
        this.add(kv[0], kv[1]);
      });
    }
  }

  static from(urlString: string): GangUrlBuilder {
    return new GangUrlBuilder(urlString);
  }

  protocol: string;
  root: string;
  parameters: {
    [key: string]: string[];
  };

  add(key: string, value: string): GangUrlBuilder {
    if (!key) throw new Error('GangUrlBuilder.add key is required');
    if (!value) throw new Error('GangUrlBuilder.add value is required');

    if (this.parameters[key] != null) {
      this.parameters[key].push(value);
    } else {
      this.parameters[key] = [value];
    }
    return this;
  }

  set(key: string, value: string | string[]): GangUrlBuilder {
    if (!key) throw new Error('GangUrlBuilder.add key is required');

    if (!value) {
      delete this.parameters[key];
    } else if (Array.isArray(value)) {
      this.parameters[key] = value as string[];
    } else {
      this.parameters[key] = [value as string];
    }
    return this;
  }

  getString(key: string): string {
    return this.parameters[key] === undefined ? undefined : this.parameters[key][0];
  }

  remove(key: string): GangUrlBuilder {
    delete this.parameters[key];
    return this;
  }

  build(): string {
    let url = `${this.protocol}:${this.root}`;

    if (this.parameters) {
      const keys = Object.keys(this.parameters);
      if (keys.length) {
        let separator = '?';
        keys.forEach((key) => {
          const values = this.parameters[key];
          values.forEach((value) => {
            url += `${separator}${key}=${value}`;
            separator = '&';
          });
        });
      }
    }
    return url;
  }
}
