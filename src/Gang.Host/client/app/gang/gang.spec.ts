import { TestBed, async } from '@angular/core/testing';

import * as Rx from 'rxjs/Rx';

import { GangModule } from './gang.module';
import { GangService } from './gang.service';
import { GangWebSocketFactory, GangWebSocket } from './gang.webSocket.factory';

import { GangUrlBuilder } from './gang.contracts';

describe('GangService', () => {

  let gangService: GangService;
  let sentMessages: any[] = [];
  let recieveMessage: (message: string) => void;
  let receiveOpen: () => void;
  let receiveClose: () => void;

  beforeEach(() => {

    const GangWebSocketFactoryMock = {
      create(
        url,
        onOpen: (e: Event) => void,
        onError: (e: Event) => void,
        onClose: (e: CloseEvent) => void): GangWebSocket {

        var messageSubject = new Rx.Subject<MessageEvent>();

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
    }

    TestBed.configureTestingModule({
      providers: [
        {
          provide: GangWebSocketFactory,
          useValue: GangWebSocketFactoryMock,
        },
        GangService
      ]
    });

    gangService = TestBed.get(GangService);
    gangService.connect('tests', 'gangId');
    receiveOpen();
  });

  it('on Host message, host is true and memberId set', done => {

    gangService.onMemberConnect.subscribe(memberId => {

      expect(gangService.memberId).toBe('MemberId');
      expect(gangService.isHost).toBe(true);
      console.log('done');
      done();
    });

    recieveMessage('HMemberId');
  });

  it('on Member message, host is false', done => {

    gangService.onMemberConnect.subscribe(memberId => {

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

    gangService.onMemberConnect.subscribe(memberId => {

      expect(() => gangService.sendState({})).toThrow();
      done();
    });

    recieveMessage('MMemberId');
  });

  it('can send state if host', done => {

    gangService.onMemberConnect.subscribe(memberId => {

      expect(() => gangService.sendState({})).not.toThrow();
      done();
    });

    recieveMessage('HMemberId');
  });

  it('sends command if not host', done => {

    gangService.onMemberConnect.subscribe(memberId => {

      gangService.sendCommand('do-it', {});
      expect(sentMessages.length).not.toBe(1);
      done();
    });

    recieveMessage('MMemberId');
  });

  it('executes command if host', done => {

    gangService.onMemberConnect.subscribe(memberId => {

      gangService.sendCommand('do-it', {});
      expect(sentMessages.length).not.toBe(0);
    });

    gangService.onCommand.subscribe(command => {

      done();
    });

    recieveMessage('HMemberId');
  });

  it('close triggers local onMemberDisconnect to allow cleanup', done => {

    gangService.onMemberConnect.subscribe(memberId => {

      receiveClose();
    });

    gangService.onMemberDisconnect.subscribe(memberId => {

      expect(memberId).toBe('MemberId');
      done();
    });

    recieveMessage('MMemberId');
  });
});

describe('GangUrlBuilder', () => {

  it('when no existing params', async(() => {
    const urlBuilder = new GangUrlBuilder('http://www.domain.com/path');
    urlBuilder.set('one', 'a');

    const url = urlBuilder.build();

    expect(url).toBe('http://www.domain.com/path?one=a');
  }));

  it('when has port number', async(() => {
    const urlBuilder = new GangUrlBuilder('http://domain:1234/path');
    urlBuilder.set('one', 'a');

    const url = urlBuilder.build();

    expect(url).toBe('http://domain:1234/path?one=a');
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
