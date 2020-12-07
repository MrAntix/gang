/**
 * Gang Http Response
 */
export interface IGangHttpResponse {
  ok: boolean;
  text(): Promise<string>;
}
