import { Component, OnInit } from '@angular/core';

import { StateService } from './todo/todo.state';
import { TodoItem } from './todo/todo.contracts';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent
  implements OnInit {

  constructor(
    private readonly stateService: StateService) { }

  get state(): TodoItem[] { return this.stateService.state; }

  createItem(textInput: HTMLInputElement) {

    this.stateService.createItem(textInput.value);

    textInput.value = '';
    textInput.focus();
  }

  completeItem(itemId: string) {

    this.stateService.completeItem(itemId);
  }

  ngOnInit(): void {

    this.stateService.connect();
  }
}
