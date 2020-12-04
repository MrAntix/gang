import { Component, h, Host, State, Listen, Fragment, Element } from '@stencil/core';
import { GangContext, getGangId, GangStore } from '@gang-js/core';

import { IAppState, IAppUser, IAppMessage, IAppMessageGroup, provideSendCommand } from '../../app/models';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true
})
export class AppHome {
  @Element() element: HTMLElement;

  gang = GangContext.service;
  logger = GangContext.logger;
  sendCommand = provideSendCommand(this);

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

    this.gang.mapEvents(this);
  }

  onGangAuthenticated(token: string) {

    this.logger('onGangAuthenticated', {
      token,
      isConnected: this.gang.isConnected,
      name: GangStore.get('name')
    });

    if (!!token
      && this.gang.isConnected
      && !!GangStore.get('name'))
      this.updatetUserName(GangStore.get('name'));
  }

  onGangState(state: Partial<IAppState>) {
    this.logger('onGangState', { state });

    state = {
      ...this.state,
      ...state
    };

    this.state = {
      messages: [],
      notifications: {},
      ...state,
      users: sortUsers(state.users || [])
    };

    this.messageGroups = sortAndGroupMessages(
      (state.messages || [])
        .concat(mapToArray(state.notifications) || [])
    );

    this.userNames = this.state.users.reduce((map, user) => {
      map[user.id] = user.name;
      return map;
    }, {});

    this.currentUser = this.state
      .users.find(user => user.isCurrent);
  }

  onGangConnectionRetry(seconds: number) {
    this.logger('onGangConnectionRetry', {})

    const message = {
      id: 'Disconnected',
      on: new Date().toISOString(),
      text: seconds !== 0
        ? `Connection was lost, retrying in ${seconds}s`
        : 'Connection was lost, click connect to try again'
    };

    this.onGangState({
      messages: [
        ...this.state.messages.filter(m => m.id !== message.id),
        message
      ]
    });
  }

  render() {

    return <Host>
      <div class="section messages">
        <ol class="messages-list" ref={el => this.messagesList = el}>
          {this.messageGroups?.map(group => <li key={group.time} class={{
            "message": true,
            "current-user": !!group.byId && group.byId === this.currentUser?.id,
            "host-bot": !this.userNames[group.byId]
          }}>
            <div class="row detail">
              <span class="text user-name">{this.userNames[group.byId] || 'Host Bot'}</span>
              <ol class="text message-text-list">
                {group.items.map(message =>
                  <li key={message.id} class={`message-text-list-item ${message.class}`}>
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

        {!this.currentUser?.name &&
          <form class="row">
            <input class="input user-name"
              autoFocus
              placeholder="(set your name)"
              onChange={(e: any) => {
                this.updatetUserName(e.target.value);
                window.setTimeout(() =>
                  this.focus('.input.message'), 600);
              }}
              value={this.currentUser?.name}
            />
            <button class="button" type="button"
            >Enter chat</button>
          </form>
        }

        {!!this.currentUser?.name &&
          <form class="row"
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

      {!!this.currentUser &&
        <div class="section users">
          <ol>
            <li class="heading">You</li>
            <li>
              <input class="input user-name"
                autoFocus
                placeholder="(set your name)"
                onChange={(e: any) => this.updatetUserName(e.target.value)}
                value={this.currentUser?.name}
              />
            </li>

            {!!this.currentUser && <Fragment>
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

  async updatetUserName(name: string) {
    this.logger('updatetUserName', { name });

    try {
      this.currentUser = {
        ...this.currentUser,
        name
      };

      await this.sendCommand('updateUserName', { name });
      GangStore.set('name', name);

    } catch (error) {
      this.currentUser = {
        ...this.currentUser,
        name: GangStore.get('name')
      };
    }
  }

  async addMessage(e: Event, text: string) {

    e.preventDefault();

    this.logger('addMessage', { text })
    if (!text) return;

    const message: IAppMessage = {
      id: getGangId(),
      text,
      on: new Date(),
      byId: this.currentUser.id,
      class: 'pending'
    }

    this.gang.setState({
      ...this.state,
      messages: [
        ...this.state.messages,
        message
      ]
    })

    await this.gang
      .sendCommand('addMessage', {
        id: getGangId(),
        text
      });

    this.logger('addMessage.done', { text })
    this.newMessageText = '';
  }

  replaceUserIds(text: string): string {
    if (!text) return;

    return this.state.users.reduce((text, user) => {

      return text.replace(`@${user.id}`, user.name);

    }, text);
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
  if (date == null || isNaN(date as any)) return ''

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
        g.byId === item.byId
        && Math.abs(g.time - time) < 10000
      );

      if (!group) {

        groups = [...groups, {
          time,
          byId: item.byId,
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

function mapToArray<T>(map: Record<string, T>) {
  if (map == null) return null;

  return Object.keys(map).map(k => map[k]).filter(v => v != null);
}
