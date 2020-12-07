import {
  Component, h, Prop, State,
  Watch
} from '@stencil/core';
import { IAppMessage, IAppMessageGroup, IAppUser } from '../../app/models';
import { arrayToMap, formatMessageDate, replaceUserIds, sortAndGroupMessages, unescapeText } from '../../app/services';

@Component({
  tag: 'app-messages',
  styleUrl: 'app-messages.css'
})
export class AppMessages {

  @Prop() value: IAppMessage[] = [];
  @Watch('value') valueSet() {
    this.messageGroups = sortAndGroupMessages(this.value);
  }

  @Prop() currentUserId: string;
  @Prop() users: IAppUser[] = [];
  @Watch('users') usersSet() {
    this.usersById = arrayToMap(this.users, u => u.id);
  }

  @State() usersById: { [id: string]: IAppUser } = {};
  @State() messageGroups: IAppMessageGroup[] = [];

  componentWillLoad(){
    this.usersSet();
    this.valueSet();
  }

  render() {

    return <ol>
      {this.messageGroups?.map(group => <li key={group.time} class={{
        "message": true,
        "current-user": !!group.byId && group.byId === this.currentUserId,
        "host-bot": !this.usersById[group.byId]
      }}>
        <div class="row detail">
          <span class="text user-name">{unescapeText(this.usersById[group.byId]?.name || 'Host Bot')}</span>
          <ol class="text message-text-list">
            {group.items.map(message =>
              <li key={message.id} data-id={message.id} class={`message-text-list-item ${message.class}`}>
                <span class="text message-text">{unescapeText(replaceUserIds(message.text, this.users))}</span>
              </li>
            )}
          </ol>
        </div>
        <div class="row info">
          <span class="message-on">{formatMessageDate(group.time)}</span>
        </div>
      </li>)}
    </ol>;
  }
}
