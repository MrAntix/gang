export interface IGangCredentialsService {
    create(options: CredentialCreationOptions): Promise<Credential>;
    get(options?: CredentialRequestOptions): Promise<Credential>;
}
