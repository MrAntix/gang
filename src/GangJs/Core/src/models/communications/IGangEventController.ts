import { Observable } from 'rxjs';
import { GangEvents } from './GangEvents';

export interface IGangEventController {
  readonly onMessage: Observable<GangEvents>;
  receive(data: ArrayBuffer): void;
}
