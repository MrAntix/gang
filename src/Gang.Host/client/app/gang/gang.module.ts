import { NgModule } from '@angular/core';

import { GangService } from './gang.service';

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
      providers: [GangService]
    }
  }}
