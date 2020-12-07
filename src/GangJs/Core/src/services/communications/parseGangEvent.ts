import { readString, readUint32 } from '../utils';
import { GangEvents, GangEventTypes } from '../../models';

export function parseGangEvent(data: ArrayBuffer): GangEvents {
  const type = readString(data, 0, 1) as GangEventTypes;

  switch (type) {
    default:
      throw new Error(`unknown message type: ${type}`);
    case GangEventTypes.Host:
    case GangEventTypes.Member:
    case GangEventTypes.Denied:
      return {
        type,
        auth: JSON.parse(readString(data, 1)),
      };
    case GangEventTypes.MemberConnected:
    case GangEventTypes.MemberDisconnected:
      return {
        type,
        memberId: readString(data, 1),
      };
    case GangEventTypes.Command:
      return {
        type,
        wrapper: {
          ...(data.byteLength > 5 ? JSON.parse(readString(data, 5)) : {}),
          sn: readUint32(data, 1),
        },
      };
    case GangEventTypes.CommandReceipt:
      return {
        type,
        rsn: readUint32(data, 1),
      };
    case GangEventTypes.State:
      return {
        type,
        state: JSON.parse(readString(data, 1)),
      };
  }
}
