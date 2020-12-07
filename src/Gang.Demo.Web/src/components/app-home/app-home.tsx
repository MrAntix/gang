import { Component, h, Host, State, Listen, Fragment, Element } from '@stencil/core';
import { GangContext, getGangId, GangStore, GangService } from '@gang-js/core';

import { IAppState, IAppUser, IAppMessage, provideSendCommand } from '../../app/models';
import { escapeText, sortUsers, unescapeText } from '../../app/services';

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

  @State() newMessageText: string;
  @State() currentUser: IAppUser;

  messagesList: HTMLOListElement;
  messagesCount: number = 0;

  @Listen('resize', { target: 'window' })
  onResize() {
    this.scrollToLastMessage();
  }

  componentWillLoad() {

    this.gang.mapEvents(this);
  }

  onGangState(state: Partial<IAppState>) {
    this.logger('home.onGangState', { state });

    state = {
      ...this.state,
      ...state
    };

    this.state = {
      ...state,
      messages: state.messages || [],
      users: sortUsers(state.users || [])
    }

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

    console.log({
      isAuthenticated: this.isAuthenticated
    })

    return <Host>
      <div class="section messages">
        <app-messages class="messages-list"
          value={this.state.messages}
          users={this.state.users}
          currentUserId={this.currentUser?.id}
        />

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
            <button class="button primary"
              disabled={!this.newMessageText}
            >Send</button>
          </form>
        }
      </div>

      {this.renderUsers()}
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
            onChange={(e: any) => this.updateUserName(escapeText(e.target.value))}
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

  componentDidRender() {
    if (this.messagesCount !== this.state.messages.length) {
      this.messagesCount = this.state.messages.length;
      this.scrollToLastMessage();
    }
  }

  async updateUserName(name: string) {

    this.logger('home.updateUserName', { name });

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
