import { Component, h } from '@stencil/core';

import { GangContext } from '../../gang';
import { mapGangEvents } from '../../gang/services';
import { IAppState } from '../../app/models';

@Component({
  tag: 'app-root',
  styleUrl: 'app-root.css',
  shadow: true
})
export class AppRoot {

  service = GangContext.service;

  componentWillLoad() {

    this.service.connect('ws', 'demo');
    mapGangEvents(this.service, this);
  }

  onState(state: IAppState) {
    console.log('app-root', { state })
  }

  onCommand(command: any) {
    console.log('app-root', { command })
  }

  onMemberConnected(memberId: any) {
    console.log('app-root', { memberId })
  }

  onMemberDisconnected(memberId: any) {
    console.log('app-root', { memberId })
  }

  render() {
    return (
      <div>
        <header>
          <h1>Gang Web</h1>
        </header>

        <main>
          <stencil-router>
            <stencil-route-switch scrollTopOffset={0}>
              <stencil-route url='/' component='app-home' exact={true} />
            </stencil-route-switch>
          </stencil-router>
        </main>
      </div>
    );
  }
}
