import { GangUrlBuilder } from './GangUrlBuilder';

describe('GangUrlBuilder', () => {

  it('when no existing params', (() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com/path');
    urlBuilder.set('one', 'a');

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com/path?one=a');
  }));

  it('when has port number', (() => {
    const urlBuilder = new GangUrlBuilder('http://domain:1234/path');
    urlBuilder.set('one', 'a');

    const url = urlBuilder.build();

    expect(url).toBe('http://domain:1234/path?one=a');
  }));

  it('when existing params', (() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com?one=a');
    urlBuilder.set('two', 'b');

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com?one=a&two=b');
  }));

  it('not set when undefined param value', (() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com?one=a');
    urlBuilder.set('two', undefined);

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com?one=a');
  }));

  it('remove existing params', (() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com?one=a');
    urlBuilder.remove('one');

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com');
  }));

  it('gets protocol', (() => {
    const urlBuilder = new GangUrlBuilder('wss://www.domain.com?one=a');

    expect(urlBuilder.protocol).toBe('wss');
  }));
});
