import { Component, h, Listen, State } from '@stencil/core';

import { GangContext } from '../../gang';
import { mapGangEvents, GangStore, getGangId } from '../../gang/services';
import { IAppState } from '../../app/models';

@Component({
  tag: 'app-root',
  styleUrl: 'app-root.css',
  shadow: true
})
export class AppRoot {

  service = GangContext.service;
  token = GangStore.get('token', () => getGangId());

  @State() isConnected: boolean = false

  @Listen('resize', { target: 'window' })
  onResize() {
    console.log('resize');
    document.documentElement
      .style.setProperty('--vh', `${window.innerHeight / 100}px`);
  }

  @Listen('visibilitychange', { target: 'document' })
  onVisibilitychange() {
    console.log('visibilitychange');

    if (document.hidden)
      this.service.disconnect();
    else
      this.service.connect('ws', 'demo', this.token);
  }

  componentWillLoad() {
    this.onResize();
    this.onVisibilitychange();

    this.service.connect('ws', 'demo', this.token);
    mapGangEvents(this.service, this);
  }

  onState(state: IAppState) {
    console.log('app-root', { state })
  }

  onCommand(command: any) {
    console.log('app-root', { command })
  }

  onMemberConnected(memberId: any) {
    console.log('app-root', { memberId });

    this.isConnected = true;
  }

  onMemberDisconnected(memberId: any) {
    console.log('app-root', { memberId });

    this.isConnected = false;
  }

  render() {
    return (
      <div>
        <header>
          <h1>Gang Web</h1>
          {!this.isConnected
            && <button
              onClick={() => this.onVisibilitychange()}
            >Connect</button>
          }
        </header>

        <main>
          {this.isConnected
            && <stencil-router>
              <stencil-route-switch scrollTopOffset={0}>
                <stencil-route url='/' component='app-home' exact={true} />
              </stencil-route-switch>
            </stencil-router>
          }
        </main>
      </div>
    );
  }
}
