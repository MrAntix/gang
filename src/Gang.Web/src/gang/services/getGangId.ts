export function getGangId(): string {

  const values = new Uint8Array(16);
  crypto.getRandomValues(values);

  return Array
    .from(values, i => ('0' + i.toString(32)).substr(-2))
    .join('');
}
