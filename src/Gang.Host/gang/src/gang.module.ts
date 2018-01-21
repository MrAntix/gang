import { NgModule } from '@angular/core';

import { GangService } from './gang.service';
import { GangWebSocketFactory } from './gang.webSocket.factory';

const components = [
];

@NgModule({
  declarations: components,
  exports: [...components],
  providers: []
})
export class GangModule {
  static forRoot() {

    return {
      ngModule: GangModule,
      providers: [GangService, GangWebSocketFactory]
    }
  }}