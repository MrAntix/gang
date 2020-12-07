export interface IGangLocationService {
  readonly host: string;
  readonly origin: string;
  readonly href: string;
  pushState(url: string): void;
}
