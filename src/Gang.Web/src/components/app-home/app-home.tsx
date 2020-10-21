import { Component, h, Host, State, Listen, Fragment, Element } from '@stencil/core';
import { GangContext, getGangId, GangStore } from '@gang-js/core';

import { IAppState, IAppUser, IAppMessage, IAppMessageGroup } from '../../app/models';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true
})
export class AppHome {
  @Element() element: HTMLElement;

  service = GangContext.service;
  logger = GangContext.logger;

  @State() state: IAppState;

  @State() messageGroups: IAppMessageGroup[];

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

    this.service.mapEvents(this);
  }

  onGangState(state: Partial<IAppState>) {
    this.logger('onState', { state });

    state = {
      ...this.state,
      ...state
    }

    this.state = {
      users: sortUsers(state.users || []),
      messages: state.messages
    }

    this.messageGroups = sortAndGroupMessages(
      (state.messages || []).concat(state.privateMessages || [])
    );

    this.userNames = this.state.users.reduce((map, user) => {
      map[user.id] = user.name;
      return map;
    }, {});

    this.currentUser = this.state
      .users.find(u => u.id === this.service.memberId);
  }

  onGangMemberConnected(memberId: string) {
    this.logger('onMemberConnected', { memberId })
    if (!memberId) return;

    if (!this.currentUser?.name
      && GangStore.get('name')) {
      this.updatetUserName(memberId, GangStore.get('name'));
    }
  }

  onGangMemberDisconnected(memberId: string) {
    this.logger('onMemberDisconnected', { memberId })

    this.onGangState({
      privateMessages: [{
        id: 'Welcome',
        userId: null,
        on: new Date().toISOString(),
        text: 'Connection was lost, retrying'
      }]
    });
    this.currentUser = null;
  }

  render() {

    return <Host>
      <div class="section messages">
        <ol class="messages-list" ref={el => this.messagesList = el}>
          {this.messageGroups?.map(group => <li class={{
            "message": true,
            "current-user": !!group.userId && group.userId === this.currentUser?.id,
            "host-bot": !this.userNames[group.userId]
          }}>
            <div class="row detail">
              <span class="text user-name">{this.userNames[group.userId] || 'Host Bot'}</span>
              <ol class="text message-text-list">
                {group.items.map(message =>
                  <li class="message-text-list-item">
                    <span class="text message-text">{this.replaceUserIds(message.text)}</span>
                  </li>
                )}
              </ol>
            </div>
            <div class="row info">
              <span class="message-on">{formatDate(group.time)}</span>
            </div>
          </li>)}
        </ol>

        {!this.currentUser?.name
          ? <form class="row">
            <input class="input user-name"
              autoFocus
              placeholder="(set your name)"
              onChange={(e: any) => {
                this.updatetUserName(this.service.memberId, e.target.value);
                window.setTimeout(() =>
                  this.focus('.input.message'), 600);
              }}
              value={this.currentUser?.name}
            />
            <button class="button"
            >Join In</button>
          </form>
          : <form class="row"
            onSubmit={e => this.addMessage(e, this.newMessageText)}
          >
            <textarea class="input message"
              autoFocus
              rows={2} placeholder="(type the message to send here)"
              value={this.newMessageText}
              onInput={(e: any) => this.newMessageText = e.target.value}
              onKeyPress={e => e.key === 'Enter' && !e.shiftKey && this.addMessage(e, this.newMessageText)}
            />
            <button class="button"
              disabled={!this.newMessageText}
            >Send</button>
          </form>
        }
      </div>

      {!!this.currentUser?.name &&
        <div class="section users">
          <ol>
            <li class="heading">You</li>
            <li>
              <input class="input user-name"
                autoFocus
                placeholder="(set your name)"
                onChange={(e: any) => this.updatetUserName(this.service.memberId, e.target.value)}
                value={this.currentUser?.name}
              />
            </li>

            {!!this.currentUser?.name && <Fragment>
              <li class="heading">Other Users</li>
              {this.state.users?.filter(u => !!u?.name && u.id !== this.currentUser?.id)
                .map(user => <li class={{
                  "user-name other text": true,
                  "is-online": user.isOnline
                }}
                >{user.name}</li>)}
            </Fragment>
            }
          </ol>
        </div>
      }
    </Host>
  }

  componentDidRender() {
    if (this.messagesCount !== this.state.messages.length) {
      this.messagesCount = this.state.messages.length;
      this.scrollToLastMessage();
    }
  }

  updatetUserName(id: string, name: string) {
    this.logger('updatetUserName', { id, name });

    if (!id) throw 'id is required'
    if (!name) throw 'name is required'

    GangStore.set('name', name);

    this.service
      .sendCommand('updateUserName', {
        id,
        name
      });
  }

  replaceUserIds(text: string): string {

    return this.state.users.reduce((text, user) => {

      return text.replace(`@${user.id}`, user.name);

    }, text);
  }

  async addMessage(e: Event, text: string) {

    e.preventDefault();

    this.logger('addMessage', { text })
    if (!text) return;

    await this.service
      .sendCommand('addMessage', {
        id: getGangId(),
        text
      });

    this.logger('addMessage.done', { text })
    this.newMessageText = '';
  }

  scrollToLastMessage() {
    this.messagesList
      ?.querySelector('.message:last-child')
      ?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
  }

  focus(selector: string) {
    this.element.shadowRoot
      .querySelector<HTMLElement>(selector)?.focus();
  }
}

const messageDateFormatter = Intl.DateTimeFormat('en-GB', {
  weekday: 'short',
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: 'numeric',
  minute: 'numeric'
}).format;
function formatDate(date: string | number) {

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

function sortAndGroupMessages(items: IAppMessage[]): IAppMessageGroup[] {

  return sortMessages(items)
    .reduce<IAppMessageGroup[]>((groups, item) => {
      const time = new Date(item.on).getTime();
      let group = groups.find(g =>
        g.userId === item.userId
        && Math.abs(g.time - time) < 10000
      );

      if (!group) {

        groups = [...groups, {
          time,
          userId: item.userId,
          items: [item]
        }]

      } else {

        groups = groups.map(
          g => g === group
            ? { ...group, items: [...group.items, item] }
            : g);
      }

      return groups;
    }, []);
}
