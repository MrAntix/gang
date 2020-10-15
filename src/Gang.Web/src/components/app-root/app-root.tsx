import { Component, Fragment, h, Listen, State } from '@stencil/core';

import { GangContext, GangStore, getGangId, GangConnectionState } from '@gang-js/core';

@Component({
  tag: 'app-root',
  styleUrl: 'app-root.css',
  shadow: true
})
export class AppRoot {

  service = GangContext.service;
  @State() token = GangStore.get('token', () => getGangId());

  @State() isConnected: boolean = false

  @Listen('resize', { target: 'window' })
  onResize() {
    document.documentElement
      .style.setProperty('--vh', `${window.innerHeight / 100}px`);
  }

  @Listen('visibilitychange', { target: 'document' })
  onVisibilitychange() {

    if (document.hidden)
      this.service.disconnect();
    else if (!this.service.isConnected)
      this.service.connect('ws', 'demo', this.token);
  }

  componentWillLoad() {
    this.onResize();

    this.service.mapEvents(this);
    this.service.connect('ws', 'demo', this.token);
  }

  onConnection(connectionState: GangConnectionState) {

    this.isConnected = connectionState === GangConnectionState.connected;
  }

  render() {
    return <Fragment>
      <section class="head">
        <header>
          <h1>Gang Web</h1>
          {!this.isConnected
            && <button
              onClick={() => this.onVisibilitychange()}
            >Connect</button>
          }
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
          <p>Demo app built on a state sharing algorithm using websockets, written in c# on net5.0 and JS client</p>
        </div>
      </section>
    </Fragment>;
  }
}
