import { Injectable } from "@angular/core";
import { GangService } from "ng-gang";

import { TodoItem } from "./todo.contracts";
import { CreateItemCommand, CompleteItemCommand } from "./todo.commands";

@Injectable()
export class StateService {

  constructor(
    public readonly gang: GangService) {
  }

  connect(): void {

    this.gang.connect('gang-relay', 'todo-demo');
    this.gang.onCommand.subscribe(wrapper => {
      console.log('onCommand', wrapper);

      const handler = `handle${wrapper.type}`;
      const newState = this[handler](wrapper.command);

      this.gang.sendState(newState);
    });

    this.gang.onState.subscribe(state => {
      console.log('onState', state);

      this.state = state.map(item => TodoItem.apply(item));
    });

    this.gang.onMemberConnect.subscribe(memberId => {
      console.log('onMemberConnect', memberId);

      this.gang.sendCommand('UserConnected', null);
    });
  }

  createItem(text: string) {

    const command = new CreateItemCommand(text, this.gang.memberId, new Date());
    this.gang.sendCommand('CreateItem', command);
  }

  completeItem(itemId: string) {

    const command = new CompleteItemCommand(itemId, this.gang.memberId, new Date());
    this.gang.sendCommand('CompleteItem', command);
  }

  state: TodoItem[] = [];

  // command handlers
  handleUserConnected(): TodoItem[] {

    return this.state;
  }

  handleCreateItem(command: CreateItemCommand): TodoItem[] {

    const item = TodoItem.create(command.text, command.userId, command.on);

    return [...this.state, item];
  }

  handleCompleteItem(command: CompleteItemCommand): TodoItem[] {

    const newState = this.state.map(item => {
      if (item.id !== command.itemId) return item;

      return item.complete(command.userId, command.on);
    });

    return newState;
  }
}
