using Gang.State;
using Gang.State.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Tests.State.Todos.Complete
{
    public sealed class CompleteTodoHandler :
        IGangCommandHandler<TodosState, CompleteTodo>
    {
        Task<GangState<TodosState>> IGangCommandHandler<TodosState, CompleteTodo>
            .HandleAsync(GangState<TodosState> state, GangCommand<CompleteTodo> command)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(command.Audit.UserId))
                errors.Add("Denied");

            return Task.FromResult(

                errors.Any()
                    ? state.RaiseErrors(errors)
                    : state.CompleteTodo(command.Data.Id, DateTimeOffset.Now)
                );
        }
    }
}
