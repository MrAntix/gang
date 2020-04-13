import { GangService } from './GangService';

export function mapGangEvents<TState>(
  service: GangService,
  component: {
    disconnectedCallback?: () => void;
    onState?: (state: TState) => void;
    onCommand?: (command: any) => void;
    onMemberConnected?: (memberId: string) => void;
    onMemberDisconnected?: (memberId: string) => void;
  }) {

  const subs = [];
  [
    'onState',
    'onCommand',
    'onMemberConnected',
    'onMemberDisconnected'
  ]
    .forEach(key => {
      if (component[key])
        subs.push(
          service[key]
            .subscribe((e: any) => component[key](e))
        );
    });

  const disconnectedCallback = component.disconnectedCallback;
  component.disconnectedCallback = () => {
    disconnectedCallback && disconnectedCallback();
    subs.forEach(sub => sub.unsubscribe());
  };
}
