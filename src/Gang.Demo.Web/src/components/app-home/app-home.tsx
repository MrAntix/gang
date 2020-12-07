import { Component, h, Host, State, Listen, Fragment, Element } from '@stencil/core';
import { GangContext, getGangId, GangStore, GangService } from '@gang-js/core';

import { IAppState, IAppUser, IAppMessage, IAppMessageGroup, provideSendCommand } from '../../app/models';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true
})
export class AppHome {
  @Element() element: HTMLElement;

  gang: GangService<IAppState> = GangContext.service;
  auth = GangContext.auth;
  logger = GangContext.logger;
  sendCommand = provideSendCommand(this);

  @State() isAuthenticated: boolean;

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

    this.logger('home.onGangAuthenticated', {
      isConnected: this.gang.isConnected,
      isAuthenticated: this.gang.isAuthenticated,
      token,
      name: GangStore.get('name')
    });

    this.isAuthenticated = this.gang.isAuthenticated;

    if (this.isAuthenticated) {
      this.removeNotification('Invite');
      this.gang.setState({
        inviteSentTo: null
      });

      if (this.gang.isConnected && !!GangStore.get('name'))
        this.updatetUserName(GangStore.get('name'));

    } else {

      this.notify({
        id: 'Welcome',
        text: 'Hello there.\nEnter your email to get an invite.\nPlease note this is a demo, no data is stored'
      });

      if (this.state?.inviteSentTo) {
        this.notify({
          id: 'Invite',
          text: `I have sent an invite to ${this.state.inviteSentTo}\nEnter the code in the email below`,
          class: 'success'
        });
      }
    }
  }

  onGangState(state: Partial<IAppState>) {
    this.logger('home.onGangState', { state });

    state = {
      ...this.state,
      ...state
    };

    this.state = {
      inviteSentTo: null,
      notifications: {},
      ...state,
      messages: state.messages || [],
      users: sortUsers(state.users || [])
    }

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
    this.logger('home.onGangConnectionRetry', {})

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
              <span class="text user-name">{unescapeText(this.userNames[group.byId] || 'Host Bot')}</span>
              <ol class="text message-text-list">
                {group.items.map(message =>
                  <li key={message.id} data-id={message.id} class={`message-text-list-item ${message.class}`}>
                    <span class="text message-text">{unescapeText(this.replaceUserIds(message.text))}</span>
                  </li>
                )}
              </ol>
            </div>
            <div class="row info">
              <span class="message-on">{formatDate(group.time)}</span>
            </div>
          </li>)}
        </ol>

        {this.isAuthenticated
          ? !this.currentUser?.name &&
          <form class="row"
            onSubmit={e => {
              e.preventDefault();

              const name = e.currentTarget['name'].value;
              this.updatetUserName(name);

              window.setTimeout(() =>
                this.focus('.input.message'), 600);
            }}>

            <input class="input fit user-name"
              name="name"
              autoFocus
              placeholder="(set your name)"
              value={this.currentUser?.name}
            />
            <button class="button"
            >Enter</button>
          </form>
          : !this.state.inviteSentTo
            ? this.renderInvite()
            : this.renderCodeEntry()

        }

        {!!this.currentUser?.name &&
          <form class="row"
            onSubmit={e => this.addMessage(e, escapeText(this.newMessageText))}
          >
            <textarea class="input fit message"
              autoFocus
              rows={2} placeholder="(type the message to send here)"
              value={this.newMessageText}
              onInput={(e: any) => this.newMessageText = e.target.value}
              onKeyPress={e => e.key === 'Enter' && !e.shiftKey && this.addMessage(e, escapeText(this.newMessageText))}
            />
            <button class="button"
              disabled={!this.newMessageText}
            >Send</button>
          </form>
        }
      </div>

      {!!this.currentUser && this.renderUsers()}
    </Host>
  }

  renderUsers() {
    return <div class="section users">
      <ol>
        <li class="heading">You</li>
        <li>
          <input class="input user-name"
            autoFocus
            placeholder="(set your name)"
            onChange={(e: any) => this.updatetUserName(escapeText(e.target.value))}
            value={unescapeText(this.currentUser?.name)}
          />
        </li>

        {!!this.currentUser && <Fragment>
          <li class="heading">Other Users</li>
          {this.state.users?.filter(u => !!u?.name && u.id !== this.currentUser?.id)
            .map(user => <li class={{
              "user-name other text": true,
              "is-online": user.isOnline
            }}
            >{unescapeText(user.name)}</li>)}
        </Fragment>
        }
      </ol>
    </div>;
  }

  renderInvite() {
    return <form class="row"
      onSubmit={e => {
        e.preventDefault();

        const emailAddress = e.currentTarget['emailAddress'].value;
        this.invite(emailAddress);

      }}>
      <input key="linkRequest" class="input fit user-email"
        name="emailAddress"
        autoFocus
        placeholder="(email address)"
      />
      <button class="button"
      >Get an Invite</button>
    </form>
  }

  renderCodeEntry() {
    return <form class="row"
      onSubmit={async e => {
        e.preventDefault();

        const code = e.currentTarget['code'].value;
        await this.authenticateCode(code);

      }}>
      <input key="link" class="input fit code"
        name="code"
        autoFocus
        placeholder="(code eg. XXX-XXX-XXX)"
      />
      <button class="button"
      >Submit Code</button>
      <a class="button"
        onClick={() => this.cancelInvite()}
      >Cancel</a>
    </form>
  }

  componentDidRender() {
    if (this.messagesCount !== this.state.messages.length) {
      this.messagesCount = this.state.messages.length;
      this.scrollToLastMessage();
    }
  }

  async invite(emailAddress: string) {

    if (await this.auth.requestLink(emailAddress)) {

      this.gang.setState({
        inviteSentTo: emailAddress
      });
      
      this.onGangAuthenticated(undefined);

    } else

      this.notify({
        id: 'Invite',
        text: `Email address is not valid, please check and try again`,
        class: 'error'
      });
  }

  cancelInvite() {

    this.gang.setState({
      inviteSentTo: undefined
    });

    this.removeNotification('Invite');
  }

  async authenticateCode(code: string) {

    const token = await this.auth.tryGetToken(code);

    if (token) await this.gang.connect({ token });
    else
      this.notify({
        id: 'Invite',
        text: 'Code was not valid or out of date, check and try again or cancel to get a new code',
        class: 'error'
      });
  }

  async updatetUserName(name: string) {

    this.logger('home.updatetUserName', { name });

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

  notify(message: IAppMessage) {

    this.gang.setState({
      notifications: {
        ...this.state?.notifications,
        [message.id]: {
          on: new Date(),
          ...message
        }
      }
    });
  }

  removeNotification(id: string) {

    this.gang.setState({
      notifications: {
        ...this.state?.notifications,
        [id]: undefined
      }
    });
  }

  async addMessage(e: Event, text: string) {

    e.preventDefault();

    this.logger('home.addMessage', { text })
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
      .sendCommand('addMessage', { id: getGangId(), text }).wait();

    this.logger('home.addMessage.done', { text })
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
      let group = groups[groups.length - 1];

      if (
        !group
        || group.byId !== item.byId
        || Math.abs(group.time - time) > 30000
      ) {

        groups = [...groups, {
          time,
          byId: item.byId,
          items: [item]
        }]

      } else {

        groups = groups.map(
          g => g === group
            ? {
              ...group,
              time,
              items: [...group.items, item]
            }
            : g);

      }

      return groups;
    }, []);


}

function mapToArray<T>(map: Record<string, T>) {
  if (map == null) return null;

  return Object.keys(map).map(k => map[k]).filter(v => v != null);
}

function escapeText(value: string): string {
  if (!value) return value;

  return unescape(encodeURIComponent(value));
}

function unescapeText(value: string): string {
  if (!value) return value;

  try {
    return decodeURIComponent(escape(value));
  } catch {
    return value;
  }
}
