/**
 * Small wrapper for location functions
 */
export class GangLocationService {
    get host() {
        return window.location.host;
    }
    get origin() {
        return `${window.location.protocol}//${window.location.host}`;
    }
    get href() {
        return window.location.href;
    }
    pushState(url) {
        window.history.pushState(null, document.title, url);
    }
    static get instance() {
        return GangLocationService._instance || (GangLocationService._instance = new GangLocationService());
    }
}
