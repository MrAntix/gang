export class CreateItemCommand {

  constructor(
    public readonly text: string,
    public readonly userId: string,
    public readonly on: Date) {}
}

export class CompleteItemCommand {

  constructor(
    public readonly itemId: string,
    public readonly userId: string,
    public readonly on: Date) {}
}
