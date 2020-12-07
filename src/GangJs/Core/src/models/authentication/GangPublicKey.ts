import { bytesToBase64Url, CBOR, viewToBuffer } from '../../services/utils';

export class GangPublicKey {
  constructor(
    public readonly keyType: number,
    public readonly algorithm: number,
    public readonly parameters: Record<string, unknown>
  ) { }

  static from(data: ArrayBuffer): GangPublicKey {
    if (data == null) return null;

    const cbor = CBOR.decode(data);
    console.log({ cbor })

    let keyType: number;
    let algorithm: number;
    const parameters = Object.keys(cbor).reduce(
      (p, k) => {
        const data = cbor[k];
        switch (k) {
          case '1': keyType = data as number; break;
          case '3': algorithm = data as number; break;
          default: {
            if (data != null) {
              if (data instanceof ArrayBuffer)
                p[k] = bytesToBase64Url(data);

              else if (ArrayBuffer.isView(data))
                p[k] = bytesToBase64Url(viewToBuffer(data as any));

              else
                p[k] = data.toString();
            }

            break;
          }
        }

        return p;
      }, {}
    );

    return new GangPublicKey(
      keyType,
      algorithm,
      parameters
    );
  }
}
