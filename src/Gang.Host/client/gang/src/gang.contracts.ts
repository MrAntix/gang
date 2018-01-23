export enum GangConnectionState {
  connecting,
  connected,
  disconnected,
  error
}

export class GangMessage {

  constructor(
    public readonly type: string,
    public readonly content: string) { }
}

export class GangUrlBuilder {

  constructor(
    urlString: string
  ) {

    const protocolSplit = urlString.split(':');
    if (protocolSplit.length < 2)
      throw 'GangUrlBuilder.ctor protocol not found ' + urlString;

    this.protocol = protocolSplit[0];

    const path = protocolSplit
      .slice(1)
      .join(':')
      .split('?');

    this.root = path[0];
    this.parameters = {};

    if (path[1]) {
      path[1]
        .split('&')
        .forEach(part => {
          var kv = part.split('=');

          this.add(kv[0], kv[1]);
        });
    }
  }

  static from(urlString: string): GangUrlBuilder {

    return new GangUrlBuilder(urlString);
  }

  protocol: string;
  root: string;
  parameters: { [key: string]: string[] };

  add(key: string, value: string): GangUrlBuilder {
    if (!key) throw 'GangUrlBuilder.add key is required';
    if (!value) throw 'GangUrlBuilder.add value is required';

    if (this.parameters[key] != undefined) {

      this.parameters[key].push(value);

    } else {

      this.parameters[key] = [value];
    }

    return this;
  }

  set(key: string, value: string | string[]): GangUrlBuilder {
    if (!key) throw 'GangUrlBuilder.add key is required';

    if (!value) {

      delete this.parameters[key];

    } else if (Array.isArray(value)) {

      this.parameters[key] = value as string[];

    } else {

      this.parameters[key] = [value as string];
    }

    return this;
  }

  remove(key: string) {

    delete this.parameters[key];

    return this;
  }

  build(): string {

    var url = `${this.protocol}:${this.root}`;

    if (this.parameters) {
      var keys = Object.keys(this.parameters);
      if (keys.length) {

        var separator = '?';
        keys.forEach(key => {

          const values = this.parameters[key];
          values.forEach(value => {

            url += `${separator}${key}=${value}`;
            separator = '&';
          })
        });
      }
    }

    return url;
  }
}

export module Gang {

  export function getId(): string {

    const values = new Uint8Array(16)
    crypto.getRandomValues(values);

    return Array
      .from(values, i => ('0' + i.toString(32)).substr(-2))
      .join('');
  }
}
