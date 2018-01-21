export class TodoItem {

  constructor(
    public id: string,
    public readonly text: string,
    public readonly audit: AuditTrail) { }

  public static apply(data: any): TodoItem {

    return new TodoItem(
      data.id, data.text,
      AuditTrail.apply(data.audit)
    )
  }

  get isCompleted(): boolean { return !!this.audit.find(TodoActions.complete); }

  public static create(text: string, userId: string, on: Date) {

    const id = uuid();
    const audit = new AuditItem(TodoActions.create, userId, on);

    return new TodoItem(id, text, new AuditTrail([audit]));
  }

  public complete(userId: string, on: Date) {

    const audit = new AuditItem(TodoActions.complete, userId, on);

    return new TodoItem(this.id, this.text, new AuditTrail([audit]));
  }
}

export enum TodoActions {
  create,
  complete
}

export class AuditItem {

  constructor(
    public readonly action: TodoActions,
    public readonly userId: string,
    public readonly on: Date,
    public readonly detail?: string) { }

  public static apply(data: any): AuditItem {

    return new AuditItem(
      data.action, data.userId,
      new Date(data.on),
      data.detail
    )
  }
}

export class AuditTrail {

  constructor(
    public readonly items: AuditItem[]) { }

  add(item: AuditItem): AuditTrail {

    return new AuditTrail([...this.items, item]);
  }

  find(action: TodoActions): AuditItem {

    return this.items.find(i => i.action === action);
  }

  findAll(action: TodoActions): AuditItem[] {

    return this.items.filter(i => i.action === action);
  }

  public static apply(data: any): AuditTrail {

    return new AuditTrail(
      data.items.map(i => AuditItem.apply(i))
    )
  }
}

export function uuid(): string {

  return String.fromCharCode.apply(null,
    crypto.getRandomValues(new Uint32Array(4))
  );
}
