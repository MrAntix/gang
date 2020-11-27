using Gang.State;
using Gang.Tests.State.Todos.Add;
using Gang.Tests.State.Todos.Complete;
using System;

namespace Gang.Tests.State.Todos
{
    public static class TodosStateExtensions
    {
        public static GangState<TodosState> AddTodo(
            this GangState<TodosState> state,
            string id)
        {
            if (state.Data.Todos.ContainsKey(id))
                state.RaiseErrors("id exists");

            return state.RaiseEvent(
                new TodoAdded(id),
                state.Data.Apply
                );
        }

        public static GangState<TodosState> CompleteTodo(
            this GangState<TodosState> state,
            string id, DateTimeOffset on)
        {
            if (!state.Data.Todos.ContainsKey(id))
                state.RaiseErrors("id does not exist");

            if (state.Data.Todos[id].CompletedOn.HasValue)
                state.RaiseErrors("already complete");

            return state.RaiseEvent(
                new TodoCompleted(id, on),
                state.Data.Apply
                );
        }
    }
}
