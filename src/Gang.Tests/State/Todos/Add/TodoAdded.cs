namespace Gang.Tests.State.Todos.Add
{
    public sealed class TodoAdded
    {
        public TodoAdded(
            string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
