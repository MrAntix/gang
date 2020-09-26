using Gang.Commands;
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
    public class WebGangHost : GangHostMemberBase
    {
        readonly IGangCommandExecutor _executor;

        public WebGangHost(
            IGangCommandExecutor executor
            )
        {
            _executor = executor
                .RegisterErrorHandler(OnErrorAsync)
                .Register<UpdateUserNameCommand>("updateUser", UpdateUser)
                .Register<AddMessageCommand>("addMessage", AddMessage);
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

        protected override async Task OnCommandAsync(byte[] data, GangMessageAudit audit)
        {
            await _executor.ExecuteAsync(data, audit);
        }

        async Task OnErrorAsync(object command, GangMessageAudit audit, Exception ex)
        {
            await NotifyAsync(
                new NotifyCommand(
                    "error", ex.Message
                ),
                new[] { audit.MemberId },
                audit.SequenceNumber
            );
        }

        WebGangHostState _state = new WebGangHostState(
          new List<WebGangUser>(),
          new List<WebGangMessage>());

        async Task RaiseStateEvent<TEvent>(
            TEvent e,
            Func<TEvent, WebGangHostState> stateChange)
        {
            Console.WriteLine($"EVENT: {e.GetType().Name}\n{JsonConvert.SerializeObject(e)}");

            _state = stateChange(e);

            if (!_state.Users.Any())
            {
                await DisconnectAsync("no-users");
                return;
            }

            await Controller.SendStateAsync(_state);
        }

        async Task UpdateUser(
            UpdateUserIsOnlineCommand command,
            GangMessageAudit audit)
        {
            if (_state.Users.Any(u => u.Id == command.Id))
            {
                var e = new WebGangUserIsOnlineUpdatedEvent(
                    command.Id,
                    command.IsOnline,
                    audit
                );

                await RaiseStateEvent(
                    e,
                    _state.Update
                    );
            }
            else
            {
                var e = new WebGangUserCreatedEvent(
                    command.Id,
                    command.IsOnline,
                    audit
                );

                await RaiseStateEvent(
                    e,
                    _state.Update
                    );
            }
        }

        async Task UpdateUser(
            UpdateUserNameCommand command, GangMessageAudit audit)
        {
            var user = _state.Users.First(u => u.Id == command.Id);
            var joined = user.Name == null;

            var e = new WebGangUserNameUpdatedEvent(
                user.Id,
                command.Name,
                audit
            );

            await RaiseStateEvent(
                e,
                _state.Update
                );

            if (joined) SendWelcome(Encoding.UTF8.GetBytes(user.Id));
        }

        async Task AddMessage(
            AddMessageCommand command,
            GangMessageAudit audit)
        {
            var e = new WebGangMessageAddedEvent(
                command.Id,
                command.Text,
                audit
            );

            await RaiseStateEvent(
                e,
                _state.Update
                );

            await NotifyAsync(
                new NotifyCommand(
                    "success", null
                ),
                new[] { audit.MemberId },
                audit.SequenceNumber
            );
        }

        async Task AddMessage(
            string message)
        {
            var e = new WebGangMessageAddedEvent(
                Guid.NewGuid().ToString("N"),
                message,
                new GangMessageAudit(this.Id)
            );

            await RaiseStateEvent(
                e,
                _state.Update
                );
        }

        async void SendWelcome(byte[] memberId)
        {
            var user = _state.Users.TryGetByIdString(memberId);

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

        async Task NotifyAsync(
            NotifyCommand command,
            byte[][] memberIds,
            short? inReplyToSequenceNumber
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
