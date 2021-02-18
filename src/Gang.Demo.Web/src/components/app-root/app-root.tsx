import { Component, Fragment, h, Listen, State } from '@stencil/core';

import { GangContext, GangConnectionState, GangService, GangAuthenticationCredential } from '@gang-js/core';
import { Commands, CommandTypes, IAppState } from '../../app/models';

@Component({
  tag: 'app-root',
  styleUrl: 'app-root.css',
  shadow: true
})
export class AppRoot {

  gang: GangService<IAppState> = GangContext.service;
  auth = GangContext.auth;
  vault = GangContext.vault;
  logger = GangContext.logger;

  @State() isConnected: boolean = false;
  @State() isAuthenticated: boolean = false;
  @State() token: string;
  @State() state: Partial<IAppState>;

  @State() credential: GangAuthenticationCredential = null;
  @State() showLinkCodeEntry: boolean = false;
  @State() email: string;
  @State() linkCode: string;

  @Listen('resize', { target: 'window' })
  onResize() {
    document.documentElement
      .style.setProperty('--vh', `${window.innerHeight / 100}px`);
  }

  @Listen('visibilitychange', { target: 'document' })
  async connect() {

    if (document.hidden)
      await this.gang.disconnect();
    else if (!this.gang.isConnected)
      await this.gang.connect()
        .catch(_ => { });
  }

  async disconnect() {
    await this.gang.disconnect('logged out');
  }

  async componentWillLoad() {

    this.token = await this.vault.get<string>('token');
    this.credential = await this.vault.get<GangAuthenticationCredential>('credential');
    this.linkCode = this.auth.tryGetLinkCodeFromUrl();
    this.showLinkCodeEntry = !!this.linkCode;

    this.gang.mapEvents(this);

    this.onResize();

    if (this.linkCode) {
      this.gang.setState({
        messages: [
          {
            id: 'Welcome',
            text: 'Please confirm your email address and click Verify'
          }
        ]
      });

    } else if (!this.token && !this.credential) {

      this.gang.setState({
        messages: [
          {
            id: 'Welcome',
            text: 'Hello, this is an example of an Authenticated Gang Chat App'
          },
          {
            id: 'Welcome',
            text: 'Get an invite to join in'
          }
        ]
      });
    }
  }

  onGangConnection(connectionState: GangConnectionState) {

    this.isConnected = connectionState === GangConnectionState.connected;
  }

  onGangState(state: Partial<IAppState>) {

    this.state = state;
  }

  async onGangAuthenticated(token: string) {

    const properties = this.auth.decodeToken(token);
    if (!properties) {
      await this.vault.delete('name');
      await this.vault.delete('email');
    }
    else {

      if (properties.name)
        await this.vault.set('name', properties.name);
      await this.vault.set('email', properties.email);
    }

    this.token = token;
    await this.vault.set('token', token);
    this.isAuthenticated = !!token;
  }

  onGangCommand(command: Commands) {
    this.logger('root.onGangCommand', command)

    switch (command.type) {
      case CommandTypes.setSettings:

        this.gang.setState({

          settings: command.data
        });

        break;
      case CommandTypes.notify:

        switch (command.data.type) {
          default:

            this.gang.setState({
              messages: [
                ...this.state.messages.filter(m => m.id !== command.data.id),
                {
                  id: command.data.id,
                  on: new Date(),
                  text: command.data.message,
                  class: `notification ${command.data.type}`
                }]
            });

            setTimeout(() => {
              this.gang.setState({

                messages: [
                  ...this.state.messages.filter(m => m.id !== command.data.id)
                ]
              })
            }, 5000);

            break;
          case 'received':
            this.logger(`command was received ${command.rsn}`);
            break;
        }

        break;
    }
  }

  async invite(email: string) {

    if (await this.auth.requestLink(email)) {

      this.email = email;
      this.showLinkCodeEntry = true;

      this.gang.setState({
        messages: [
          {
            id: 'InviteSent',
            text: `An invite with an access code has been sent to ${this.email}`
          }
        ]
      })

    }
  }

  async authenticate() {
    console.info('authenticate', {
      isAuthenticated: this.isAuthenticated,
      token: this.token
    })

    if (!this.credential) {

      const challenge = await this.auth.tryGetChallenge(this.token);

      // register
      this.credential = await this.auth.registerCredential(this.token, challenge);

      await this.vault.set('credential', this.credential);

    } else {

      try {

        // login
        this.token = await this.auth.validateCredential(this.credential);

      } catch (err) {
        console.error(err);

        await this.vault.delete('credential');
        this.credential = null;
      }
    }

    if (this.token)
      await this.gang.connect({
        path: '/ws', gangId: 'demo',
        token: this.token
      });
  }

  async logout() {

    this.gang.authenticate(null);

  }

  render() {

    return <Fragment>
      <section class="head">
        <header>
          <h1>Gang Web</h1>
        </header>

        <div>
          <p class="row fit">
            <a href="https://github.com/MrAntix/gang">github.com/MrAntix/gang</a>

            {this.isAuthenticated && <a class="right"
              onClick={() => this.logout()}>Logout</a>
            }
          </p>
        </div>
      </section>

      <section class="body">
        {this.isAuthenticated
          ? this.renderHomePage()
          : !this.token && !this.credential
            ? this.renderLink()
            : this.renderAuthenticate()
        }
      </section>

      <section class="foot">
        <div>
          <p>A state sharing algorithm using websockets, written in c# on net5.0 and JS client</p>
        </div>
      </section>
    </Fragment>;
  }

  renderHomePage() {
    return <main>
      <stencil-router>
        <stencil-route-switch scrollTopOffset={0}>
          <stencil-route url='/' component='app-home' exact={true} />
        </stencil-route-switch>
      </stencil-router>
    </main>;
  }

  renderAuthenticate() {

    console.info('renderNotLoggedIn', {
      token: this.token
    });

    return <button class="button primary" type="button"
      onClick={() => this.authenticate()}>Login</button>;
  }

  renderLink() {

    console.info('renderLink', {
      token: this.token
    });

    if (!this.showLinkCodeEntry)

      return <section class="link">
        <app-messages class="messages-list"
          value={this.state.messages}
          users={this.state.users}
        />

        <form class="form"
          onSubmit={e => {
            e.preventDefault();

            const email = e.currentTarget['email'].value;
            this.invite(email);

          }}>

          <div class="row fill">
            <input key="linkRequest" class="input fit user-email"
              name="email"
              placeholder="(email address)"
              autoFocus type="email" inputMode="email"
              required
              value={this.email}
              onChange={(e: any) => this.email = e.target.value}
            />
          </div>

          <div class="row fill">
            <button class="button primary right"
            >Get an access code</button>
          </div>

          <div class="row fill">
            <a class="button right"
              onClick={() => {
                this.showLinkCodeEntry = true;
              }}
            >I have an access code &gt;</a>
          </div>
        </form>
      </section>

    return <section class="link">
      <app-messages class="messages-list"
        value={this.state.messages}
        users={this.state.users}
      />

      <form class="form"
        onSubmit={async e => {
          e.preventDefault();

          this.token = await this.auth.validateLink(this.email, this.linkCode);

          if (this.token)
            this.showLinkCodeEntry = false;

        }}>

        <div class="row fit">
          <input key="email" class="input fit user-email"
            name="email"
            placeholder="(email address)"
            autoFocus type="email" inputMode="email"
            required
            value={this.email}
            onChange={(e: any) => this.email = e.target.value}
          />
        </div>

        <div class="row fit">
          <input key="code" class="input fit code"
            name="code"
            placeholder="(code eg. XXX-XXX)"
            autoFocus
            required
            value={this.linkCode}
            onChange={(e: any) => this.linkCode = e.target.value}
          />
        </div>

        <div class="row fill">
          <button class="button primary right"
          >Verify</button>
        </div>

        <div class="row fill">
          <a class="button"
            onClick={() => {
              this.showLinkCodeEntry = false;
            }}
          >&lt; Get another access code</a>
        </div>
      </form>
    </section>
  }
}
