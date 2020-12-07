import { GangEvents, IWebSocket, WebSocketData } from '../../models';
import { stringToBytes } from '../utils';
import { createGangWebSocket } from './createGangWebSocket';

describe('createGangWebSocket', () => {
  it('subscribe', () => {
    let received: GangEvents = null;
    const fakeSocket = new FakeSocket();

    const socket = createGangWebSocket(null, null, null, null, () => fakeSocket);

    socket.subscribe((m) => {
      received = m;
    });

    fakeSocket.onmessage(
      new MessageEvent('message', {
        data: stringToBytes('+MemberId'),
      })
    );

    expect(received).not.toBeNull();
  });

  it('close', () => {
    let received: CloseEvent = null;

    const socket = createGangWebSocket(
      null,
      null,
      null,
      (e) => (received = e),
      () => new FakeSocket()
    );

    socket.close();

    expect(received).not.toBeNull();
  });

  class FakeSocket implements IWebSocket {
    sent: WebSocketData[] = [];

    binaryType: BinaryType;
    onclose: (this: IWebSocket, ev: CloseEvent) => void;
    onerror: (this: IWebSocket, ev: Event) => void;
    onmessage: (this: IWebSocket, ev: MessageEvent<unknown>) => void;
    onopen: (this: IWebSocket, ev: Event) => void;
    send(data: WebSocketData): void {
      this.sent = [...this.sent, data];
    }
    close(_code?: number, reason?: string): void {
      this.onclose(new CloseEvent('close', { reason }));
    }
  }
});
