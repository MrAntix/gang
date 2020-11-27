namespace Gang.Tests.State.Todos.Add
{
    public sealed class AddTodo
    {
        public AddTodo(
            string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
