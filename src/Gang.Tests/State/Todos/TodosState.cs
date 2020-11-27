using Gang.Tests.State.Todos.Add;
using Gang.Tests.State.Todos.Complete;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.Tests.State.Todos
{
    public sealed class TodosState
    {
        public TodosState(
            IEnumerable<Todo> todos = null
            )
        {
            Todos = todos
                ?.ToImmutableDictionary(i => i.Id)
                ?? ImmutableDictionary<string, Todo>.Empty;
        }

        public TodosState() : this(null) { }

        public IImmutableDictionary<string, Todo> Todos { get; }

        public TodosState Apply(
            TodoAdded data
            )
        {
            return new TodosState(
                Todos.Values.Append(new Todo(data.Id))
                );
        }

        public TodosState Apply(
            TodoCompleted data
            )
        {
            var todo = Todos[data.Id];
            return new TodosState(
                Todos.Remove(data.Id)
                    .Values
                    .Append(todo.SetCompletedOn(data.On))
                );
        }
    }
}
