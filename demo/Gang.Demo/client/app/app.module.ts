import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { GangModule } from 'ng-gang';

import { AppComponent } from './app.component';
import { StateService } from './todo/todo.state';


@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    GangModule.forRoot()
  ],
  providers: [StateService],
  bootstrap: [AppComponent]
})
export class AppModule { }
