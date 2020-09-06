import { Subject } from 'rxjs';

import { GangWebSocket } from '../models';
import { GangService } from './GangService';

describe('GangService', () => {
  let gangService: GangService;
  let recieveMessage: (message: string) => void;
  let receiveOpen: () => void;
  let receiveClose: () => void;

  const sentMessages: unknown[] = [];

  beforeEach(() => {
    function webSocketFactoryMock(
      _url,
      onOpen: (e: Event) => void,
      _onError: (e: Event) => void,
      onClose: (e: CloseEvent) => void
    ): GangWebSocket {
      const messageSubject = new Subject<MessageEvent>();

      messageSubject.subscribe(() => undefined);

      recieveMessage = (message) => {
        if (typeof message !== 'string') message = JSON.stringify(message);

        const blob = new Blob([message], {
          type: 'text/plain',
        });

        messageSubject.next(new MessageEvent('message', { data: blob }));
      };

      receiveOpen = () => onOpen(null);
      receiveClose = () =>
        onClose(new CloseEvent('close', { reason: 'disconnected' }));

      return new GangWebSocket(messageSubject, (data) => {
        sentMessages.push(data);
      });
    }

    gangService = new GangService(webSocketFactoryMock);
    gangService.connect('tests', 'gangId');
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

  it('onMemberConnected host message, host is true and memberId set', (done) => {
    gangService.onMemberConnected.subscribe(() => {
      expect(gangService.memberId).toBe('MemberId');
      expect(gangService.isHost).toBe(true);
      done();
    });

    recieveMessage('HMemberId');
  });

  it('onMemberConnected member message, host is false', (done) => {
    gangService.onMemberConnected.subscribe(() => {
      expect(gangService.memberId).toBe('MemberId');
      expect(gangService.isHost).toBe(false);
      done();
    });

    recieveMessage('MMemberId');
  });

  it('onCommand message', (done) => {
    gangService.onCommand.subscribe((command) => {
      expect(command).not.toBeNull();
      done();
    });

    recieveMessage('C{}');
  });

  it('onState current state on subscribe', (done) => {
    const currentState = { current: true };
    recieveMessage(`S${JSON.stringify(currentState)}`);

    gangService.onState.subscribe((state) => {
      if (state === undefined) return;

      expect(state).toEqual(currentState);
      done();
    });
  });

  it('onState new state ', (done) => {
    const newState = { new: true };

    gangService.onState.subscribe((state) => {
      if (state === undefined) return;

      expect(state).toEqual(newState);
      done();
    });

    recieveMessage(`S${JSON.stringify(newState)}`);
  });

  it('waitForState', () => {
    const newState = { new: true };

    recieveMessage(`S${JSON.stringify(newState)}`);

    expect(
      async () => await gangService.waitForState<typeof newState>((s) => s?.new)
    ).resolves;
  });

  it('waitForState timeout', () => {
    const newState = { new: false };

    recieveMessage(`S${JSON.stringify(newState)}`);

    expect(
      async () =>
        await gangService.waitForState<typeof newState>((s) => s?.new, {
          timeout: 10,
        })
    ).rejects;
  });

  it('cannot send state if not host', (done) => {
    gangService.onMemberConnected.subscribe(() => {
      expect(() => gangService.sendState({})).toThrow();
      done();
    });

    recieveMessage('MMemberId');
  });

  it('can send state if host', (done) => {
    gangService.onMemberConnected.subscribe(() => {
      expect(() => gangService.sendState({})).not.toThrow();
      done();
    });

    recieveMessage('HMemberId');
  });

  it('sends command if not host', (done) => {
    gangService.onMemberConnected.subscribe(() => {
      gangService.sendCommand('do-it', {});
      expect(sentMessages.length).not.toBe(1);
      done();
    });

    recieveMessage('MMemberId');
  });

  it('executes command if host', (done) => {
    gangService.onMemberConnected.subscribe(() => {
      gangService.sendCommand('do-it', {});
      expect(sentMessages.length).not.toBe(0);
    });

    gangService.onCommand.subscribe(() => {
      done();
    });

    recieveMessage('HMemberId');
  });

  it('close triggers local onMemberDisconnect to allow cleanup', (done) => {
    gangService.onMemberConnected.subscribe(() => {
      receiveClose();
    });

    gangService.onMemberDisconnected.subscribe((memberId) => {
      expect(memberId).toBe('MemberId');
      done();
    });

    recieveMessage('MMemberId');
  });
});
