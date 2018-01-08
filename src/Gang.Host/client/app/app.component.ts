import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { GangService } from './gang/gang.service';

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

    this.gang.sendCommand(
      'setUserName',
      {
        id: this.gang.id,
        name: this.nameInput.nativeElement.value
      });
  }

  constructor(private gang: GangService) {

    this.state = {
      users: []
    }
  }

  get id() { return this.gang.id; }
  state: any;

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

    this.gang.connect('ws', 'demo');

    this.gang.onConnect.subscribe(id => {

      this.gang.sendCommand(
        'addUser',
        {
          id: id,
          name: this.nameInput.nativeElement.value
        });
    });

    this.gang.onDisconnect.subscribe(id => {

      if (id === this.gang.id) {

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
