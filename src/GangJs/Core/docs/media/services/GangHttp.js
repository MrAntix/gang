/**
 * Small wrapper for HTTP functions
 */
export class GangHttp {
    constructor(rootPath) {
        this.rootPath = rootPath;
    }
    fetch(url, init) {
        return fetch(`${this.rootPath}${url}`, init);
    }
}
