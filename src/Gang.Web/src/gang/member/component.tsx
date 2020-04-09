import { Component } from '@stencil/core';
import { tap } from 'rxjs/operators';

import { GangService, createGangWebSocket } from '../services';

@Component({
  tag: 'gang-member',
  styleUrl: 'component.css',
  shadow: true
})
export class GangMemberComponent {

  service = new GangService(createGangWebSocket);

  componentWillLoad() {

    this.service.onState.pipe(
      tap(state => console.log({ state }))
    )
      .subscribe();

    this.service.onMemberConnect.pipe(
      tap(memberId => console.log({ memberId }))
    )
      .subscribe();

    this.service.connect('ws', 'demo');
  }
}
