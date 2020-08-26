import { GangService } from './GangService';

export function mapGangEvents<TState>(
  service: GangService,
  component: {
    disconnectedCallback?: () => void;
    onState?: (state: TState) => void;
    onCommand?: (command: unknown) => void;
    onMemberConnected?: (memberId: string) => void;
    onMemberDisconnected?: (memberId: string) => void;
  }
): void {
  const subs: { unsubscribe: () => undefined }[] = [];
  ['onState', 'onCommand', 'onMemberConnected', 'onMemberDisconnected'].forEach(
    (key) => {
      if (component[key])
        subs.push(service[key].subscribe((e: unknown) => component[key](e)));
    }
  );

  const disconnectedCallback = component.disconnectedCallback;
  component.disconnectedCallback = () => {
    if (disconnectedCallback) disconnectedCallback();
    subs.forEach((sub) => sub.unsubscribe());
  };
}
