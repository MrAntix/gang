namespace Gang.Tests.State.Todos.Complete
{
    public sealed class CompleteTodo
    {
        public CompleteTodo(
            string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
