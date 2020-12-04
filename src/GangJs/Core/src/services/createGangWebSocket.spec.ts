import { createGangWebSocket, IWebSocket } from './createGangWebSocket';

describe('createGangWebSocket', () => {

  it('subscribe', () => {

    let received: MessageEvent = null;
    const fakeSocket = new FakeSocket()

    const socket = createGangWebSocket(
      null, () => { }, () => { }, () => { },
      () => fakeSocket
    );

    socket.subscribe(m => {
      received = m;
    });

    fakeSocket.onmessage(new MessageEvent('message', {}));

    expect(received).not.toBeNull();
  });

  it('close', () => {

    let received: CloseEvent = null;

    const socket = createGangWebSocket(
      null, () => { }, () => { },
      e => received = e,
      () => new FakeSocket()
    );

    socket.close();

    expect(received).not.toBeNull();
  });

  class FakeSocket implements IWebSocket {
    binaryType: BinaryType;
    onclose: (this: IWebSocket, ev: CloseEvent) => any;
    onerror: (this: IWebSocket, ev: Event) => any;
    onmessage: (this: IWebSocket, ev: MessageEvent<any>) => any;
    onopen: (this: IWebSocket, ev: Event) => any;
    send(_data: string | ArrayBuffer | SharedArrayBuffer | Blob | ArrayBufferView): void {

    }
    close(_code?: number, reason?: string): void {
      this.onclose(new CloseEvent('close', { reason }));
    }
  };
});
