import { TestBed, async } from '@angular/core/testing';

import { GangModule } from './gang.module';
import { GangUrlBuilder } from './gang.contracts';

describe('GangUrlBuilder', () => {

  it('when no existing params', async(() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com');
    urlBuilder.set('one', 'a');

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com?one=a');
  }));

  it('when existing params', async(() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com?one=a');
    urlBuilder.set('two', 'b');

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com?one=a&two=b');
  }));

  it('not set when undefined param value', async(() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com?one=a');
    urlBuilder.set('two', undefined);

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com?one=a');
  }));

  it('remove existing params', async(() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com?one=a');
    urlBuilder.remove('one');

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com');
  }));

  it('gets protocol', async(() => {
    const urlBuilder = new GangUrlBuilder('wss://www.domain.com?one=a');

    expect(urlBuilder.protocol).toBe('wss');
  }));
});
