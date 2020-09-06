import { getGangId } from './getGangId';

describe('getGangId', () => {
  it('not null', () => {
    const id = getGangId();
    console.log(id);

    expect(id).not.toBeNull();
  });

  it('is 36 chars and dashes', () => {
    const id = getGangId();

    expect(id.length).toBe(36);
  });

  it('looks like a guid', () => {
    const id = getGangId();

    expect(id[14]).toBe('4');
  });
});
