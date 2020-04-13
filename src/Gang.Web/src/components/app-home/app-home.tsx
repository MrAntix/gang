import { Component, h, Host, State } from '@stencil/core';

import { GangContext } from '../../gang';
import { mapGangEvents, getGangId, GangStore } from '../../gang/services';
import { IAppState, IAppUser, IAppMessage } from '../../app/models';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true
})
export class AppHome {

  service = GangContext.service;
  @State() messages: IAppMessage[] = [];
  @State() newMessageText: string;

  @State() users: IAppUser[] = [];
  @State() currentUser: IAppUser;
  @State() userNames: { [id: string]: string } = {};

  messagesList: HTMLOListElement;
  messagesCount: number = 0;

  componentWillLoad() {

    mapGangEvents(this.service, this);
  }

  onState(state: IAppState) {
    console.log('app-home', { state })

    this.users = sortUsers(state.users || []);
    this.messages = state.messages || [];

    this.currentUser = this.users.find(
      u => u.id === this.service.memberId
    );
    this.userNames = this.users.reduce((map, user) => {
      map[user.id] = user.name;
      return map;
    }, {});
  }

  onMemberConnected(id: string) {
    this.updateUser({
      id,
      name: this.currentUser?.name || GangStore.get('name')
    })
  }

  render() {
    return <Host>
      {!!this.currentUser?.name &&
        <div class="section messages">
          <ol class="messages-list" ref={el => this.messagesList = el}>
            {this.messages?.map(message => <li class={{
              "message": true,
              "current-user": message.userId === this.currentUser?.id
            }}>
              <div class="row detail">
                <span class="text user-name">{this.userNames[message.userId]}</span>
                <div class="text message-text">{message.text}</div>
              </div>
              <div class="row info">
                <span class="text message-on">{formatDate(message.on)}</span>
              </div>
            </li>)}
          </ol>

          <form class="row"
            onSubmit={e => this.addMessage(e, this.newMessageText)}
          >
            <textarea class="input message"
              rows={2} placeholder="(type the message to send here)"
              value={this.newMessageText}
              onInput={(e: any) => this.newMessageText = e.target.value}
              onKeyPress={e => e.key === 'Enter' && !e.shiftKey && this.addMessage(e, this.newMessageText)}
            />
            <button class="button"
              disabled={!this.newMessageText}
            >Send</button>
          </form>
        </div>
      }
      <div class="section users">
        <ol>
          <li class="heading">You</li>
          <li>
            <input class="input user-name"
              placeholder="(set your name)"
              onChange={(e: any) => this.updateUserName(e.target.value)}
              value={this.currentUser?.name}
            />
          </li>
          <li class="heading">Other Users</li>
          {this.users?.filter(u => u !== this.currentUser)
            .map(user => <li class={{
              "user-name other text": true,
              "is-online": user.isOnline
            }}
            >{user?.name || '(anon)'}</li>)}
        </ol>
      </div>
    </Host>
  }

  componentDidRender() {

    if (this.messagesCount !== this.messages.length) {
      const lastMessage = this.messagesList.querySelector('li:last-child');
      lastMessage.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }
  }

  updateUser(change: Partial<IAppUser>) {

    this.service
      .sendCommand('updateUser', {
        ...this.currentUser,
        ...change
      });
  }

  updateUserName(name: string) {

    GangStore.set('name', name);
    this.updateUser({ name });
  }

  addMessage(e: Event, text: string) {
    e.preventDefault();

    console.debug('addMessage', { text })
    if (!text) return;

    this.service
      .sendCommand('addMessage', {
        id: getGangId(),
        text
      });

    this.newMessageText = '';
  }
}

const messageDateFormatter = Intl.DateTimeFormat('en-GB', {
  weekday: 'short',
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: 'numeric',
  minute: 'numeric',
  second: 'numeric'
}).format;
function formatDate(date: string) {

  return messageDateFormatter(new Date(date));
}

function sortUsers(users: IAppUser[]): IAppUser[] {
  const sorted = [...users];

  sorted.sort((a, b) => a.isOnline && b.isOnline
    ? a.name?.localeCompare(b.name)
    : a.isOnline ? -1 : 1);
  return sorted;
}
