import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { GangModule } from './gang/gang.module';

import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    GangModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
