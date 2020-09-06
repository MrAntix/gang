import { getGangId } from './getGangId';

describe('getGangId', () => {
  it('not null', () => {
    const id = getGangId();

    expect(id).not.toBeNull();
  });

  it('is 32 chars', () => {
    const id = getGangId();

    expect(id.length).toBe(32);
  });

  it('looks like a guid', () => {
    const id = getGangId();

    expect(id[12]).toBe('4');
  });
});
