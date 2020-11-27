using System;

namespace Gang.Tests.State.Todos
{
    public sealed class Todo : IHasGangIdString
    {
        public Todo(
            string id,
            DateTimeOffset? completedOn = null)
        {
            Id = id;
            CompletedOn = completedOn;
        }

        public string Id { get; }
        public DateTimeOffset? CompletedOn { get; }

        public Todo SetCompletedOn(DateTimeOffset on)
        {
            return new Todo(Id, on);
        }
    }
}
