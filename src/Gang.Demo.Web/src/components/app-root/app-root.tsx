import { Component, Fragment, h, Listen, State } from '@stencil/core';

import { GangContext, GangStore, GangConnectionState } from '@gang-js/core';
import { Commands, CommandTypes } from '../../app/models';

@Component({
  tag: 'app-root',
  styleUrl: 'app-root.css',
  shadow: true
})
export class AppRoot {

  gang = GangContext.service;
  auth = GangContext.auth;
  logger = GangContext.logger;

  @State() token: string;

  @State() isConnected: boolean = false

  @Listen('resize', { target: 'window' })
  onResize() {
    document.documentElement
      .style.setProperty('--vh', `${window.innerHeight / 100}px`);
  }

  @Listen('visibilitychange', { target: 'document' })
  connect() {
    this.logger('connect', { token: this.token })

    if (document.hidden)
      this.gang.disconnect();
    else if (!this.gang.isConnected)
      this.gang.connect('ws', 'demo', this.token);
  }

  async componentWillLoad() {
    this.logger('componentWillLoad', { token: this.token })

    if (this.token = await this.auth.tryLinkInUrl())
      this.connect();

    this.gang.mapEvents(this);
  }

  onGangConnection(connectionState: GangConnectionState) {

    this.isConnected = connectionState === GangConnectionState.connected;
  }

  onGangAuthenticate(token: string) {
    this.logger('onGangAuthenticate', { token })

    GangStore.set('properties', atob(token.substr(0, token.indexOf('.'))))
    this.token = token;
  }

  async componentDidLoad() {
    this.logger('componentDidLoad', { token: this.token })

    this.connect();
    this.onResize();
  }

  onGangCommand(command: Commands) {
    this.logger('onGangCommand', command)

    switch (command.type) {
      case CommandTypes.notify:

        switch (command.data.type) {
          default:
            console.log(command.data);

            this.gang.setState({

              notifications: {
                [command.data.id]: {
                  id: command.data.id,
                  on: new Date(),
                  text: command.data.message,
                  class: `notification ${command.data.type}`
                }
              }
            })

            setTimeout(() => {
              this.gang.setState({

                notifications: {
                  [command.data.id]: undefined
                }
              })
            }, 5000);

            break;
          case 'received':
            console.log(`command was received ${command.rsn}`);
            break;
        }

        break;
    }
  }

  render() {
    return <Fragment>
      <section class="head">
        <header>
          <h1>Gang Web</h1>
        </header>

        <div>
          <p><a href="https://github.com/MrAntix/gang">github.com/MrAntix/gang</a></p>
          {!this.isConnected
            && <button class="connect-button" type="button"
              onClick={() => this.connect()}
            >Connect</button>
          }
        </div>
      </section>

      <section class="body">
        <main>
          <stencil-router>
            <stencil-route-switch scrollTopOffset={0}>
              <stencil-route url='/' component='app-home' exact={true} />
            </stencil-route-switch>
          </stencil-router>
        </main>
      </section>

      <section class="foot">
        <div>
          <p>Demo app built on a state sharing algorithm using websockets, written in c# on net5.0 and JS client</p>
        </div>
      </section>
    </Fragment>;
  }
}
