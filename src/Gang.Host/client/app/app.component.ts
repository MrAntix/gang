import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { GangService } from './gang/gang.service';

const gangId = 'demo';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent
  implements OnInit {

  @ViewChild('name')
  nameInput: ElementRef;

  changeName() {

    if (!this.gang.isConnected) return;

    this.gang.sendCommand(
      'setUserName',
      {
        id: this.gang.memberId,
        name: this.nameInput.nativeElement.value
      });
  }

  constructor(
    private gang: GangService,
    private httpClient: HttpClient) {

    this.state = {
      users: []
    }
  }

  get connectText() { return this.gang.retryingIn ? `retry (${this.gang.retryingIn})` : 'connect'; }
  get isConnected() { return this.gang.isConnected; }
  get memberId() { return this.gang.memberId; }
  state: any;

  connect() {

    this.gang.connect('ws', gangId);
  }

  disconnect(memberId: string) {

    this.httpClient
      .get(`/disconnect?gangId=${gangId}&memberId=${memberId}`)
      .toPromise();
  }

  addUser(command: any): any {

    return {
      users: [...this.state.users, command]
    };
  }

  removeUser(command: any): any {

    return {
      users: this.state.users.filter(u => u.id !== command.id)
    };
  }

  setUserName(command: any): any {

    return {
      users: this.state.users
        .map(u => u.id === command.id
          ? command
          : u)
    };
  }

  ngOnInit(): void {

    console.debug('AppComponent.ngOnInit');

    var users = [];

    this.gang.onMemberConnect.subscribe(id => {

      this.gang.sendCommand(
        'addUser',
        {
          id: id,
          name: this.nameInput.nativeElement.value
        });
    });

    this.gang.onMemberDisconnect.subscribe(id => {

      if (id === this.gang.memberId) {

        this.state.users = [];
        return;
      }

      this.gang.sendCommand(
        'removeUser',
        {
          id: id
        });
    });

    this.gang.onCommand.subscribe(wrapper => {

      this.gang.sendState(this[wrapper.type](wrapper.command));
    });

    this.gang.onState.subscribe(state => {

      this.state = state;
    });

  }
}
