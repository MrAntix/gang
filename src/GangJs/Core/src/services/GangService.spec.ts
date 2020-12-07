import { Subject } from 'rxjs';

import { GangWebSocket } from '../models';
import { parseGangEvent } from './communications';
import { GangService } from './GangService';

describe('GangService', () => {
  type IState = { three?: boolean };

  let gangService: GangService<IState>;
  let recieveMessage: (type: string, message: string, sn?: number) => void;
  let receiveOpen: () => void;
  let receiveClose: () => void;

  const sentMessages: ArrayBuffer[] = [];
  const AUTH_MESSAGE = '{"memberId":"MemberId"}';

  function webSocketFactoryMock(
    _url: string,
    onOpen: (e: Event) => void,
    _onError: (e: Event) => void,
    onClose: (e: CloseEvent) => void
  ): GangWebSocket {
    const messageSubject = new Subject<MessageEvent>();

    messageSubject.subscribe(() => undefined);

    recieveMessage = (type, message, sn) => {
      if (typeof message !== 'string') message = JSON.stringify(message);

      const parts: BlobPart[] = [type];
      if (sn != null) {
        const snPart = new Uint32Array(1);
        snPart[0] = sn;
        parts.push(snPart);
      }
      parts.push(message);

      const reader = new FileReader();
      reader.onload = () => messageSubject.next(new MessageEvent('message', { data: reader.result }));

      const blob = new Blob(parts);
      reader.readAsArrayBuffer(blob);
    };

    receiveOpen = () => onOpen(null);
    receiveClose = () => onClose(new CloseEvent('close', { reason: 'disconnected' }));

    return new GangWebSocket(parseGangEvent, messageSubject, (data) => sentMessages.push(data));
  }

  beforeEach(() => {
    GangService.setState(null);

    gangService = new GangService(webSocketFactoryMock);
    gangService.connect({
      path: '/tests',
      gangId: 'gangId',
    });
    receiveOpen();
  });

  it('onConnection message', (done) => {
    gangService.onConnection.subscribe(() => {
      expect(gangService.isConnected).toBe(true);
      done();
    });
  });

  it('onConnection message close', (done) => {
    receiveClose();

    gangService.onConnection.subscribe(() => {
      expect(gangService.isConnected).toBe(false);
      done();
    });
  });

  it('onAuthenticated host message, host is true and memberId set', (done) => {
    gangService.onAuthenticated.subscribe(() => {
      expect(gangService.memberId).toBe('MemberId');
      expect(gangService.isHost).toBe(true);
      done();
    });

    recieveMessage('H', AUTH_MESSAGE);
  });

  it('onAuthenticated member message, host is false', (done) => {
    gangService.onAuthenticated.subscribe(() => {
      expect(gangService.memberId).toBe('MemberId');
      expect(gangService.isHost).toBe(false);
      done();
    });

    recieveMessage('M', AUTH_MESSAGE);
  });

  it('onCommand message', (done) => {
    gangService.onCommand.subscribe((command) => {
      expect(command).not.toBeNull();
      done();
    });

    recieveMessage('C', '{}', 1);
  });

  it('onCommand after execute', (done) => {
    const type = 'TYPE';
    const data = {};

    gangService.onCommand.subscribe((c) => {
      expect(c.type).toEqual(type);
      expect(c.data).toEqual(data);
      done();
    });

    gangService.executeCommand(type, data);
  });

  it('onState current state on subscribe', (done) => {
    const currentState = { current: true };
    recieveMessage('S', JSON.stringify(currentState));

    gangService.onState.subscribe((state) => {
      if (state == null) return;

      expect(state).toEqual(currentState);
      done();
    });
  });

  it('onState new state', (done) => {
    const newState = { new: true };

    gangService.onState.subscribe((state) => {
      if (state == null) return;

      expect(state).toEqual(newState);
      done();
    });

    recieveMessage('S', JSON.stringify(newState));
  });

  it('onState merges', (done) => {
    const firstState = { one: true, two: true };
    const secondState = { two: false, three: true };

    gangService.onState.subscribe((state: IState) => {
      if (state?.three) {
        expect(state).toEqual({
          one: true,
          two: false,
          three: true,
        });
        done();
      }
    });

    recieveMessage('S', JSON.stringify(firstState));
    recieveMessage('S', JSON.stringify(secondState));
  });

  it('waitForState', () => {
    const newState = { new: true };

    recieveMessage('S', JSON.stringify(newState));

    expect(async () => await gangService.waitForState<typeof newState>((s) => s?.new)).resolves;
  });

  it('waitForState timeout', () => {
    const newState = { new: false };

    recieveMessage('S', JSON.stringify(newState));

    expect(
      async () =>
        await gangService.waitForState<typeof newState>((s) => s?.new, {
          timeout: 10,
        })
    ).rejects;
  });

  it('cannot send state if not host', (done) => {
    gangService.onAuthenticated.subscribe(() => {
      expect(() => gangService.sendState({})).toThrow();
      done();
    });

    recieveMessage('M', AUTH_MESSAGE);
  });

  it('can send state if host', (done) => {
    gangService.onAuthenticated.subscribe(() => {
      expect(() => gangService.sendState({})).not.toThrow();
      done();
    });

    recieveMessage('H', AUTH_MESSAGE);
  });

  it('sends command if not host', (done) => {
    gangService.onAuthenticated.subscribe(() => {
      gangService.sendCommand('do-it', {});
      expect(sentMessages.length).not.toBe(1);
      done();
    });

    recieveMessage('M', AUTH_MESSAGE);
  });

  it('sends commands with sequence number', (done) => {
    gangService.onAuthenticated.subscribe(async () => {
      gangService.sendCommand('do-it', {});
      gangService.sendCommand('do-it-again', {});

      const d = new DataView(sentMessages[3], 0);
      expect(d.getUint32(0, true)).toBe(2);
      done();
    });

    recieveMessage('M', AUTH_MESSAGE);
  });

  it('send and wait for returning sequence number', (done) => {
    gangService.onAuthenticated.subscribe(() => {
      gangService
        .sendCommand('do-it', {})
        .wait()
        .then(() => done());

      recieveMessage('C', '{"rsn":1}', 1);
    });

    recieveMessage('M', AUTH_MESSAGE);
  });

  it('executes command if host', (done) => {
    gangService.onAuthenticated.subscribe(() => {
      gangService.sendCommand('do-it', {});
      expect(sentMessages.length).not.toBe(0);
    });

    gangService.onCommand.subscribe(() => {
      done();
    });

    recieveMessage('H', AUTH_MESSAGE);
  });
});
