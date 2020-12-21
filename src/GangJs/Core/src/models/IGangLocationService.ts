
export interface IGangLocationService {
  readonly host: string;
  readonly href: string;
  pushState(url: string): void;
}
