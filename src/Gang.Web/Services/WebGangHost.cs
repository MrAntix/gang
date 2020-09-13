using Gang.Commands;
using Gang.Web.Services.Commands;
using Gang.Web.Services.State;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

            await UpdateUser(new UpdateUserIsOnlineCommand(
                userId, true
                ));
        }

        protected override async Task OnMemberDisconnectAsync(byte[] memberId)
        {
            var userId = Encoding.UTF8.GetString(memberId);

            await UpdateUser(new UpdateUserIsOnlineCommand(
                userId, false
                ));
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

        async Task SetState(WebGangHostState state)
        {
            if (!state.Users.Any())
            {
                await DisconnectAsync("no-users");
                return;
            }

            _state = state;

            await Controller.SendStateAsync(state);
        }

        async Task UpdateUser(
            UpdateUserIsOnlineCommand command)
        {
            var user = _state.Users.FirstOrDefault(u => u.Id == command.Id);
            if (user == null)
            {
                user = new WebGangUser(command.Id, null, command.IsOnline);
                await SetState(
                    new WebGangHostState(
                        _state.Users.Add(user),
                        _state.Messages
                    ));
            }
            else
                await SetState(
                    new WebGangHostState(
                      _state.Users.Replace(user, user.Update(command)),
                      _state.Messages
                    ));
        }

        async Task UpdateUser(
            UpdateUserNameCommand command)
        {
            var user = _state.Users.First(u => u.Id == command.Id);
            var joined = user.Name == null;

            await SetState(
                new WebGangHostState(
                  _state.Users.Replace(user, user.Update(command)),
                  _state.Messages
                ));

            if (joined) SendWelcome(Encoding.UTF8.GetBytes(user.Id));
        }

        async Task AddMessage(
            AddMessageCommand command,
            GangMessageAudit audit = null)
        {
            await SetState(
                new WebGangHostState(
                  _state.Users,
                  _state.Messages.Add(
                    new WebGangMessage(
                      command.Id, DateTimeOffset.UtcNow,
                      audit?.MemberId.GangToString(),
                      command.Text)
                    ).TakeLast(10)
                ));

            if (audit == null) return;

            await NotifyAsync(
                new NotifyCommand(
                    "success", null                    
                ),
                new[] { audit.MemberId },
                audit.SequenceNumber
            );
        }

        async Task AddMessage(
            string message,
            GangMessageAudit audit = null)
        {
            await AddMessage(
                new AddMessageCommand(Guid.NewGuid().ToString("N"), message),
                audit);
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
            short? sn
            )
        {
            await Controller.SendCommandAsync(
                NotifyCommand.TYPE_NAME,
                command,
                memberIds,
                sn
                );
        }
    }
}
