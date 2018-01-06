import { Component, OnInit } from '@angular/core';
import { GangService } from './gang/gang.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent
  implements OnInit {

  constructor(private gang: GangService) { }

  ngOnInit(): void {

    console.debug('AppComponent.ngOnInit');

    this.gang.onConnect.subscribe(e => console.log(e));
    this.gang.connect('ws', 'client');

  }
}
