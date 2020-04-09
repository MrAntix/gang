import { Subject } from 'rxjs';

import { GangWebSocket } from '../contracts';
import { GangService } from './GangService';

describe('GangService', () => {

  let gangService: GangService;
  let sentMessages: any[] = [];
  let recieveMessage: (message: string) => void;
  let receiveOpen: () => void;
  let receiveClose: () => void;

  beforeEach(() => {

    function webSocketFactoryMock(
      _url,
      onOpen: (e: Event) => void,
      _onError: (e: Event) => void,
      onClose: (e: CloseEvent) => void): GangWebSocket {

      var messageSubject = new Subject<MessageEvent>();

      messageSubject.subscribe(e => {
      });

      recieveMessage = (message) => {

        if (typeof message !== 'string')
          message = JSON.stringify(message);

        var blob = new Blob(
          [message],
          {
            type: 'text/plain'
          });

        messageSubject.next(new MessageEvent(
          'message', { data: blob })
        );
      };

      receiveOpen = () => onOpen(null);
      receiveClose = () => onClose(new CloseEvent('close', { reason: 'disconnected' }))

      return new GangWebSocket(messageSubject, data => {

        sentMessages.push(data);
      });
    }

    gangService = new GangService(webSocketFactoryMock);
    gangService.connect('tests', 'gangId');
    receiveOpen();
  });

  it('on Host message, host is true and memberId set', done => {

    gangService.onMemberConnect.subscribe(() => {

      expect(gangService.memberId).toBe('MemberId');
      expect(gangService.isHost).toBe(true);
      console.log('done');
      done();
    });

    recieveMessage('HMemberId');
  });

  it('on Member message, host is false', done => {

    gangService.onMemberConnect.subscribe(() => {

      expect(gangService.memberId).toBe('MemberId');
      expect(gangService.isHost).toBe(false);
      done();
    });

    recieveMessage('MMemberId');
  });

  it('on Disconnect message, host is true', done => {

    gangService.onMemberDisconnect.subscribe(memberId => {

      expect(memberId).toBe('OtherMemberId');
      expect(gangService.isHost).toBe(true);
      done();
    });

    recieveMessage('DOtherMemberId');
  });

  it('on Command message', done => {

    gangService.onCommand.subscribe(command => {

      expect(command).not.toBeNull();
      done();
    })

    recieveMessage('C{}');
  });

  it('on State message', done => {

    gangService.onState.subscribe(state => {

      expect(state).not.toBeNull();
      done();
    });

    recieveMessage('S{}');
  });

  it('cannot send state if not host', done => {

    gangService.onMemberConnect.subscribe(() => {

      expect(() => gangService.sendState({})).toThrow();
      done();
    });

    recieveMessage('MMemberId');
  });

  it('can send state if host', done => {

    gangService.onMemberConnect.subscribe(() => {

      expect(() => gangService.sendState({})).not.toThrow();
      done();
    });

    recieveMessage('HMemberId');
  });

  it('sends command if not host', done => {

    gangService.onMemberConnect.subscribe(() => {

      gangService.sendCommand('do-it', {});
      expect(sentMessages.length).not.toBe(1);
      done();
    });

    recieveMessage('MMemberId');
  });

  it('executes command if host', done => {

    gangService.onMemberConnect.subscribe(() => {

      gangService.sendCommand('do-it', {});
      expect(sentMessages.length).not.toBe(0);
    });

    gangService.onCommand.subscribe(() => {

      done();
    });

    recieveMessage('HMemberId');
  });

  it('close triggers local onMemberDisconnect to allow cleanup', done => {

    gangService.onMemberConnect.subscribe(() => {

      receiveClose();
    });

    gangService.onMemberDisconnect.subscribe(memberId => {

      expect(memberId).toBe('MemberId');
      done();
    });

    recieveMessage('MMemberId');
  });
});
