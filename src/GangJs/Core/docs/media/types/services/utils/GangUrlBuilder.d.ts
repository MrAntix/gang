export declare class GangUrlBuilder {
    constructor(urlString: string);
    static from(urlString: string): GangUrlBuilder;
    protocol: string;
    root: string;
    parameters: {
        [key: string]: string[];
    };
    add(key: string, value: string): GangUrlBuilder;
    set(key: string, value: string | string[]): GangUrlBuilder;
    getString(key: string): string;
    remove(key: string): GangUrlBuilder;
    build(): string;
}
