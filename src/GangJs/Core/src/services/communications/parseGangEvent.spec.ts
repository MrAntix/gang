import { setBytes, stringToBytes, toBytes, uint32ToBytes } from '../utils';
import { GangEventTypes, GangEvent } from '../../models';
import { parseGangEvent } from './parseGangEvent';

describe('parseGangEvent', () => {
  function call<T extends GangEventTypes>(message: {
    type: T;
    content?: ArrayBuffer;
    sequenceNumber?: number;
  }): GangEvent<T> {
    const length = 1 + (message.sequenceNumber != null ? 4 : 0) + message.content.byteLength;
    const buffer = new ArrayBuffer(length);

    setBytes(buffer, 0, stringToBytes(message.type));

    if (message.sequenceNumber != null) setBytes(buffer, 1, uint32ToBytes(message.sequenceNumber));

    if (message.content) setBytes(buffer, message.sequenceNumber != null ? 5 : 1, message.content);

    const e = parseGangEvent(buffer);

    return e as GangEvent<T>;
  }

  it('recieves host message', async () => {
    const e = call({
      type: GangEventTypes.Host,
      content: toBytes({ memberId: 'MemberId' }),
    });

    expect(e.auth.memberId).toBe('MemberId');
  });

  it('recieves member message', async () => {
    const e = call({
      type: GangEventTypes.Member,
      content: toBytes({ memberId: 'MemberId' }),
    });

    expect(e.auth.memberId).toBe('MemberId');
  });

  it('recieves denied message', async () => {
    const e = call({
      type: GangEventTypes.Denied,
      content: toBytes({ memberId: 'MemberId' }),
    });

    expect(e.auth.memberId).toBe('MemberId');
  });

  it('recieves member connected', async () => {
    const e = call({
      type: GangEventTypes.MemberConnected,
      content: stringToBytes('MemberId'),
    });

    expect(e.memberId).toBe('MemberId');
  });

  it('recieves member disconnected', async () => {
    const e = call({
      type: GangEventTypes.MemberDisconnected,
      content: stringToBytes('MemberId'),
    });

    expect(e.memberId).toBe('MemberId');
  });

  it('recieves command', async () => {
    const e = call({
      type: GangEventTypes.Command,
      content: toBytes({ type: 'TYPE' }),
      sequenceNumber: 99,
    });

    expect(e.wrapper.sn).toBe(99);
    expect(e.wrapper.type).toBe('TYPE');
  });

  it('recieves command receipt', async () => {
    const e = call({
      type: GangEventTypes.CommandReceipt,
      content: uint32ToBytes(99),
    });

    expect(e.rsn).toBe(99);
  });

  it('recieves state', async () => {
    const state = { key: 'VALUE' };
    const e = call({
      type: GangEventTypes.State,
      content: toBytes(state),
    });

    expect(e.state).toEqual(state);
  });
});
