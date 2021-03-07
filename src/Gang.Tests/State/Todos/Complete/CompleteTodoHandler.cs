using Gang.State;
using Gang.State.Commands;
using System;
using System.Threading.Tasks;

namespace Gang.Tests.State.Todos.Complete
{
    public sealed class CompleteTodoHandler :
        IGangCommandHandler<TodosState, CompleteTodo>
    {
        Task<GangState<TodosState>> IGangCommandHandler<TodosState, CompleteTodo>
            .HandleAsync(GangState<TodosState> state, GangCommand<CompleteTodo> command)
        {
            return Task.FromResult(

                state
                    .Assert(!string.IsNullOrWhiteSpace(command.Audit.UserId), "DENIED")
                    .CompleteTodo(command.Data.Id, DateTimeOffset.Now)
                    .AddResult(command.Audit.MemberId,
                        new GangNotify("Well done", type: GangNotificationTypes.Success))
                );
        }
    }
}
