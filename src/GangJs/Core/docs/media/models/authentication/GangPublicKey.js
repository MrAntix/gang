import { bytesToBase64Url, CBOR, viewToBuffer } from '../../services/utils';
export class GangPublicKey {
    constructor(keyType, algorithm, parameters) {
        this.keyType = keyType;
        this.algorithm = algorithm;
        this.parameters = parameters;
    }
    static from(data) {
        if (data == null)
            return null;
        const cbor = CBOR.decode(data);
        console.log({ cbor });
        let keyType;
        let algorithm;
        const parameters = Object.keys(cbor).reduce((p, k) => {
            const data = cbor[k];
            switch (k) {
                case '1':
                    keyType = data;
                    break;
                case '3':
                    algorithm = data;
                    break;
                default: {
                    if (data != null) {
                        if (data instanceof ArrayBuffer)
                            p[k] = bytesToBase64Url(data);
                        else if (ArrayBuffer.isView(data))
                            p[k] = bytesToBase64Url(viewToBuffer(data));
                        else
                            p[k] = data.toString();
                    }
                    break;
                }
            }
            return p;
        }, {});
        return new GangPublicKey(keyType, algorithm, parameters);
    }
}
