import { NgModule } from '@angular/core';

import { GangService } from './gang.service';

const components = [
];

@NgModule({
  declarations: components,
  exports: [...components],
  providers: [
    GangService
  ]
})
export class GangModule { }
