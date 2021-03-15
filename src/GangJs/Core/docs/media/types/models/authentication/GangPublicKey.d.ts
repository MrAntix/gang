export declare class GangPublicKey {
    readonly keyType: number;
    readonly algorithm: number;
    readonly parameters: Record<string, unknown>;
    constructor(keyType: number, algorithm: number, parameters: Record<string, unknown>);
    static from(data: ArrayBuffer): GangPublicKey;
}
