import { IGangVault, IGangVaultData, IGangVaultSettings } from '../../models';
import { getRandomBytes } from '../utils';

const onError = (e: Event) => {
  alert(`${e}`);
  throw new Error(`${e}`);
};

export class GangVault implements IGangVault {
  constructor(private readonly settings: IGangVaultSettings) {
    this.keyParameters = {
      name: 'AES-GCM',
      length: 256,
    };

    this.getCryptoKey().then(async (cryptoKey) => {
      this.cryptoKey = cryptoKey;
      if (!cryptoKey) await this.setCryptoKey();
    });
  }

  private cryptoKey: CryptoKey;

  public readonly keyParameters: AesKeyGenParams;

  private execute(mode: IDBTransactionMode, func: (store: IDBObjectStore) => void, onError: (e: Event) => void) {
    const open = indexedDB.open(this.settings.name, 1);

    open.onupgradeneeded = () => {
      open.result.createObjectStore(this.settings.store);
    };

    open.onerror = (e) => onError(e);

    open.onsuccess = () => {
      const db = open.result;
      const tx = db.transaction(this.settings.store, mode);
      const store = tx.objectStore(this.settings.store);

      func(store);

      tx.onerror = (e) => onError(e);
      tx.oncomplete = () => db.close();
    };
  }

  async deleteCryptoKey(): Promise<void> {
    await this.delete(this.settings.key);
  }

  async getCryptoKey(): Promise<CryptoKey> {
    return await this.get<CryptoKey>(this.settings.key);
  }

  async setCryptoKey(): Promise<CryptoKey> {
    try {
      this.cryptoKey = (await globalThis.crypto.subtle.generateKey(this.keyParameters, false, [
        'encrypt',
        'decrypt',
      ])) as CryptoKey;

      await this.set(this.settings.key, this.cryptoKey);

      return this.cryptoKey;
    } catch (err) {
      onError(err);
    }
  }

  get<T>(key: string, getDefault: () => T = null): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      this.execute(
        'readonly',
        async (store) => {
          const get = store.get(key);
          get.onsuccess = () => {
            if (get.result === undefined) {
              resolve(getDefault && getDefault());

              return;
            }

            resolve(get.result);
          };
        },
        (e) => reject(e)
      );
    });
  }

  async getEncrypted(key: string, seed?: ArrayBuffer): Promise<ArrayBuffer> {
    try {
      const data = await this.get<IGangVaultData>(key);

      return await globalThis.crypto.subtle.decrypt(
        {
          name: this.keyParameters.name,
          iv: data.iv,
          tagLength: 128,
          additionalData: seed,
        },
        this.cryptoKey,
        data.value
      );
    } catch (err) {
      onError(err);
    }
  }

  set<T>(key: string, value: T): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      this.execute(
        'readwrite',
        (store) => {
          const put = store.put(value, key);
          put.onsuccess = () => {
            resolve();
          };
        },
        (e) => reject(e)
      );
    });
  }

  async setEncrypted(key: string, value: ArrayBuffer, seed?: ArrayBuffer): Promise<void> {
    try {
      const iv = getRandomBytes(12);
      const data = {
        value: await globalThis.crypto.subtle.encrypt(
          {
            name: this.keyParameters.name,
            iv,
            tagLength: 128,
            additionalData: seed,
          },
          this.cryptoKey,
          value
        ),
        iv,
      };

      await this.set(key, data);
    } catch (err) {
      onError(err);
    }
  }

  async delete(key: string): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      this.execute(
        'readwrite',
        (store) => {
          const put = store.delete(key);
          put.onsuccess = () => {
            resolve();
          };
        },
        (e) => reject(e)
      );
    });
  }
}
