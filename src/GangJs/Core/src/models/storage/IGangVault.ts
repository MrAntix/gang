export interface IGangVault {
  deleteCryptoKey(): Promise<void>;
  getCryptoKey(): Promise<CryptoKey>;
  setCryptoKey(): Promise<CryptoKey>;
  get<T>(key: string, getDefault?: () => T): Promise<T>;
  getEncrypted(key: string, seed?: ArrayBuffer): Promise<ArrayBuffer>;
  set<T>(key: string, value: T): Promise<void>;
  setEncrypted(key: string, value: ArrayBuffer, seed?: ArrayBuffer): Promise<void>;
  delete(key: string): Promise<void>;
}
