import { Component, h, Host, State, Listen } from '@stencil/core';
import { GangContext, mapGangEvents, getGangId, GangStore } from '@gang-js/core';

import { IAppState, IAppUser, IAppMessage } from '../../app/models';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true
})
export class AppHome {

  service = GangContext.service;

  @State() state: IAppState = {
    users: [],
    messages: [],
    privateMessages: []
  }

  @State() newMessageText: string;

  @State() currentUser: IAppUser;
  @State() userNames: { [id: string]: string } = {};

  messagesList: HTMLOListElement;
  messagesCount: number = 0;

  @Listen('resize', { target: 'window' })
  onResize() {
    this.scrollToLastMessage();
  }

  componentWillLoad() {

    mapGangEvents(this.service, this);
  }

  onState(state: IAppState) {

    state = {
      ...this.state,
      ...state
    }

    this.state = {
      users: sortUsers(state.users || []),
      messages: sortMessages(
        (state.messages || []).concat(state.privateMessages || [])
      )
    }

    this.currentUser = this.state.users
      .find(u => u.id === this.service.memberId)
      || {
      id: this.service.memberId,
      name: GangStore.get('name'),
      isOnline: false
    };
    this.userNames = this.state.users.reduce((map, user) => {
      map[user.id] = user.name;
      return map;
    }, {});
  }

  render() {
    return <Host>
      {!!this.currentUser?.name &&
        <div class="section messages">
          <ol class="messages-list" ref={el => this.messagesList = el}>
            {this.state.messages?.map(message => <li class={{
              "message": true,
              "current-user": message.userId === this.currentUser?.id
            }}>
              <div class="row detail">
                <span class="text user-name">{this.userNames[message.userId] || 'Host Bot'}</span>
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
          {this.state.users?.filter(u => u !== this.currentUser)
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
    if (this.messagesCount !== this.state.messages.length) {
      this.messagesCount = this.state.messages.length;
      this.scrollToLastMessage();
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

  scrollToLastMessage() {
    this.messagesList
      ?.querySelector('li:last-child')
      ?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
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

function sortUsers(items: IAppUser[]): IAppUser[] {
  const sorted = [...items];

  sorted.sort((a, b) => a.isOnline && b.isOnline
    ? a.name?.localeCompare(b.name)
    : a.isOnline ? -1 : 1);
  return sorted;
}

function sortMessages(items: IAppMessage[]): IAppMessage[] {
  const sorted = [...items];

  sorted.sort((a, b) => new Date(a.on).getTime() - new Date(b.on).getTime());
  return sorted;
}
