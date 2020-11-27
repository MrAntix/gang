using System;

namespace Gang.Tests.State.Todos.Complete
{
    public sealed class TodoCompleted
    {
        public TodoCompleted(
            string id,
            DateTimeOffset on)
        {
            Id = id;
            On = on;
        }

        public string Id { get; }
        public DateTimeOffset On { get; }
    }
}
