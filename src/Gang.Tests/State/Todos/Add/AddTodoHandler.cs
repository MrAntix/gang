using Gang.State;
using Gang.State.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Tests.State.Todos.Add
{
    public sealed class AddTodoHandler :
        IGangCommandHandler<TodosState, AddTodo>
    {
        Task<GangState<TodosState>> IGangCommandHandler<TodosState, AddTodo>
            .HandleAsync(GangState<TodosState> state, GangCommand<AddTodo> command)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(command.Audit.UserId))
                errors.Add("Denied");

            return Task.FromResult(

                errors.Any()
                    ? state.RaiseErrors(errors)
                    : state.AddTodo(command.Data.Id)
                );
        }
    }
}
