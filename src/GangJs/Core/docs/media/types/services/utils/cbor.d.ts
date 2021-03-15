declare function encode(value: string): ArrayBuffer;
declare function decode(data: ArrayBuffer, tagger?: any, simpleValue?: any): any;
export declare const CBOR: {
    encode: typeof encode;
    decode: typeof decode;
};
export {};
