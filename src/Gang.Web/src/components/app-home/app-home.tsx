import { Component, h, Host, State } from '@stencil/core';

import { GangContext } from '../../gang';
import { mapGangEvents, getGangId } from '../../gang/services';
import { IAppState, IAppUser, IAppMessage } from '../../app/models';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true
})
export class AppHome {

  service = GangContext.service;
  @State() users: IAppUser[] = [];
  @State() messages: IAppMessage[] = [];
  @State() currentUser: IAppUser;
  @State() userNames: { [id: string]: string } = {};

  messageInput: HTMLTextAreaElement;
  messagesList: HTMLOListElement;
  messagesCount: number = 0;

  componentWillLoad() {

    mapGangEvents(this.service, this);
  }

  onState(state: IAppState) {
    console.log('app-home', { state })

    this.users = state.users || [];
    this.messages = state.messages || [];
    this.currentUser = state?.users?.find(
      u => u.id === this.service.memberId
    );
    this.userNames = state?.users?.reduce((map, user) =>
      (map[user.id] = user.name) && map,
      {});
  }

  onMemberConnected(id) {
    this.updateUser({
      id,
      name: this.currentUser?.name ?? 'Anonymous'
    })
  }

  render() {
    return <Host>
      <div class="section users">
        <ol>
          <li>
            <input class="input user-name"
              onChange={(e: any) => this.updateUser({
                name: e.target.value
              })}
              value={this.currentUser?.name}
            />
          </li>

          {this.users?.filter(u => u !== this.currentUser)
            .map(user => <li class="text">{user?.name}</li>)}
        </ol>
      </div>

      <div class="section messages">
        <ol class="messages-list" ref={el => this.messagesList = el}>
          {this.messages?.map(message => <li class="message">
            <div class="row message-audit">
              <span class="text message-user-name">{this.userNames[message.userId]}</span>
              <span class="text message-on">{formatDate(message.on)}</span>
            </div>
            <div class="text message-text">{message.text}</div>
          </li>)}
        </ol>

        <form class="row"
          onSubmit={e => this.addMessage(e, this.messageInput.value)}
        >
          <textarea class="input message" ref={el => this.messageInput = el}
            rows={2}
          />
          <button class="button">Send</button>
        </form>
      </div>
    </Host>
  }

  componentDidRender() {
    if (this.messagesCount !== this.messages.length) {
      this.messagesList.scrollTop = this.messagesList.scrollHeight;
      this.messagesCount = this.messages.length
    }
  }

  updateUser(change: Partial<IAppUser>) {

    this.service
      .sendCommand('updateUser', {
        ...this.currentUser,
        ...change
      });
  }

  addMessage(e: Event, text: string) {
    e.preventDefault();

    this.service
      .sendCommand('addMessage', {
        id: getGangId(),
        text
      });

    this.messageInput.value = '';
  }
}

const messageDateFormatter = Intl.DateTimeFormat(
  'default', {}).format;
function formatDate(date: string) {

  return messageDateFormatter(new Date(date));
}
