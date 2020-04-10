import { Component, h, Host, State } from '@stencil/core';

import { GangContext } from '../../gang';
import { mapGangEvents } from '../../gang/services';
import { IAppState, IAppUser } from '../../app/models';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true
})
export class AppHome {

  service = GangContext.service;
  @State() state: IAppState;
  @State() currentUser: IAppUser;

  componentWillLoad() {

    mapGangEvents(this.service, this);
  }

  onState(state: IAppState) {
    console.log('app-home', { state })

    this.state = state;
    this.currentUser = state?.users?.find(
      u => u.id === this.service.memberId
    );
  }

  onMemberConnected(id) {
    this.updateUser({
      id,
      name: this.currentUser?.name ?? 'Anonymous'
    })
  }

  render() {
    return <Host>
      <ol>
        <li>
          <input
            onChange={(e: any) => this.updateUser({
              name: e.target.value
            })}
            value={this.currentUser?.name}
          />
        </li>

        {this.state?.users?.filter(u => u !== this.currentUser)
          .map(user => <li>{user?.name}</li>)}
      </ol>

      <pre>
        State: {
          JSON.stringify(this.state)
        }</pre>
    </Host>
  }

  updateUser(change: Partial<IAppUser>) {

    this.service
      .sendCommand('updateUser', {
        ...this.currentUser,
        ...change
      });
  }
}
