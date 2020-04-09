import { getGangId } from './getGangId';

describe('Gang', () => {

  it('can getId', (() => {

    const id = getGangId();
    expect(id).not.toBeNull();

    console.info('Gang.getId', id);
  }));

})
