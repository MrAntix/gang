using Gang.Commands;
using Gang.Contracts;
using Gang.Events;
using Gang.Members;
using Gang.Web.Services.Commands;
using Gang.Web.Services.Events;
using Gang.Web.Services.State;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangHost : GangStatefulHostBase<WebGangHostState>
    {

        public WebGangHost(
            IGangCommandExecutor<WebGangHost> commandExecutor
            )
        {
            Use(commandExecutor
                .RegisterErrorHandler(OnCommandErrorAsync)
                );
        }

        protected override Task OnConnectAsync()
        {
            SetState(new WebGangHostState(
              new List<WebGangUser>(),
              new List<WebGangMessage>())
            );

            ApplyStateEvents(
                new[]{
                    new GangEventWrapper(
                        new WebGangMessageAddedEvent("Welcome", "Gang Chat Started"),
                        new GangMessageAudit(Id, null, 1, DateTimeOffset.Now)
                        )
                });

            return base.OnConnectAsync();
        }

        protected override async Task OnMemberConnectAsync(byte[] memberId)
        {
            var userId = Encoding.UTF8.GetString(memberId);

            await UpdateUser(
                new UpdateUserIsOnlineCommand(userId, true),
                new GangMessageAudit(memberId, null)
                );
        }

        protected override async Task OnMemberDisconnectAsync(byte[] memberId)
        {
            var userId = Encoding.UTF8.GetString(memberId);

            await UpdateUser(
                new UpdateUserIsOnlineCommand(userId, false),
                new GangMessageAudit(memberId, null)
                );
        }

        async Task OnCommandErrorAsync(object command, GangMessageAudit audit, Exception ex)
        {
            await NotifyAsync(
                new NotifyCommand(
                    "error", ex.Message
                ),
                new[] { audit.MemberId },
                audit.SequenceNumber
            );
        }

        protected override async Task OnStateEventAsync(
            object e, GangMessageAudit a)
        {
            Console.WriteLine(
                $"EVENT: {e.GetType().Name}" +
                $"\n{JsonConvert.SerializeObject(e)}" +
                $"\n{JsonConvert.SerializeObject(a)}");

            await Controller.SendStateAsync(State);
        }

        async Task UpdateUser(
            UpdateUserIsOnlineCommand command,
            GangMessageAudit audit)
        {
            if (State.Users.Any(u => u.Id == command.Id))
            {
                var e = new WebGangUserIsOnlineUpdatedEvent(
                    command.Id,
                    command.IsOnline
                );

                await RaiseStateEventAsync(e, audit.MemberId, State.Apply);
            }
            else
            {
                var e = new WebGangUserCreatedEvent(
                    command.Id,
                    command.IsOnline
                );

                await RaiseStateEventAsync(e, audit.MemberId, State.Apply);
            }
        }

        async Task AddMessage(
            string message)
        {
            var e = new WebGangMessageAddedEvent(
                Guid.NewGuid().ToString("N"),
                message
            );

            await RaiseStateEventAsync(e, Id, State.Apply);
        }

        public async Task SendWelcome(byte[] memberId)
        {
            var user = State.Users.TryGetByIdString(memberId);

            await AddMessage(
                $"{user.Name} Joined"
                );

            await Task.Delay(3000);

            await Controller.SendStateAsync(new
            {
                PrivateMessages = new[] {
                            new WebGangMessage(
                                "Welcome", DateTimeOffset.Now, null,
                                $"Hello {user.Name}, welcome to the gang")
                              }
            },
            new[] { memberId });
        }

        public async Task NotifyAsync(
            NotifyCommand command,
            byte[][] memberIds,
            uint? inReplyToSequenceNumber
            )
        {
            await Controller.SendCommandAsync(
                NotifyCommand.TYPE_NAME,
                command,
                memberIds,
                inReplyToSequenceNumber
                );
        }
    }
}
