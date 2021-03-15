export function clean(obj, test = (v) => v == null) {
    if (obj == null)
        return obj;
    return Object.entries(obj).reduce((n, [k, v]) => (test(v) ? n : ((n[k] = v), n)), {});
}
