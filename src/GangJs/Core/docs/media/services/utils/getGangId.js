export const getGangId = (function () {
    const map = [];
    const dash = [3, 5, 7, 9];
    for (let i = 0; i < 256; i++) {
        map[i] = (i < 16 ? '0' : '') + i.toString(16);
    }
    return () => {
        const r = crypto.getRandomValues(new Uint8Array(16));
        r[6] = (r[6] & 0x0f) | 0x40;
        r[8] = (r[8] & 0x3f) | 0x80;
        return [...r.entries()].map((v, i) => `${map[v[1]]}${dash.includes(i) ? '-' : ''}`).join('');
    };
})();
