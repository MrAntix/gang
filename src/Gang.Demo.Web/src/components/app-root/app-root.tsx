import { Component, Fragment, h, Listen, State } from '@stencil/core';

import { GangContext, GangStore, GangConnectionState, GangService } from '@gang-js/core';
import { Commands, CommandTypes, IAppState } from '../../app/models';

@Component({
  tag: 'app-root',
  styleUrl: 'app-root.css',
  shadow: true
})
export class AppRoot {

  gang: GangService<IAppState> = GangContext.service;
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
  async connect() {
    this.logger('root.connect', { token: this.token })

    if (document.hidden)
      await this.gang.disconnect();
    else if (!this.gang.isConnected)
      await this.gang.connect({
        path: '/ws', gangId: 'demo',
        token: this.token
      })
        .catch(_ => { });
  }

  async componentWillLoad() {
    this.logger('root.componentWillLoad', { token: this.token })

    this.token = await this.auth.tryGetTokenFromUrl();

    await this.connect();

    this.gang.mapEvents(this);

    this.onResize();
    await this.connect();
  }

  onGangConnection(connectionState: GangConnectionState) {

    this.isConnected = connectionState === GangConnectionState.connected;
  }

  async onGangAuthenticated(token: string) {
    this.logger('root.onGangAuthenticated', { token });

    const properties = this.auth.decodeToken(token);
    if (!properties) {
      GangStore.set('name');
      GangStore.set('emailAddress');
    }
    else {
      let credentials;
      if (GangStore.get('registered')) {
        credentials = await this.auth.getCredential(properties, token);

      } else {
        credentials = await this.auth.register(properties, token);
        GangStore.set('registered', 'yes');
      }

      this.logger({ properties, credentials });

      if (properties.name) GangStore.set('name', properties.name);
      GangStore.set('emailAddress', properties.emailAddress);
    }

    this.token = token;
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

              notifications: {
                [command.data.id]: {
                  id: command.data.id,
                  on: new Date(),
                  text: command.data.message,
                  class: `notification ${command.data.type}`
                }
              }
            });

            setTimeout(() => {
              this.gang.setState({

                notifications: {
                  [command.data.id]: undefined
                }
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

  render() {
    return <Fragment>
      <section class="head">
        <header>
          <h1>Gang Web</h1>
        </header>

        <div>
          <p><a href="https://github.com/MrAntix/gang">github.com/MrAntix/gang</a></p>
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
          <p>A state sharing algorithm using websockets, written in c# on net5.0 and JS client</p>
        </div>
      </section>
    </Fragment>;
  }
}
