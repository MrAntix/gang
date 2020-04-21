using Gang.Serialization;
using Gang.Web.Services.Commands;
using Gang.Web.Services.State;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangHost : IGangMember
    {
        readonly TaskCompletionSource<bool> _connected;
        readonly IImmutableDictionary<string, Func<JObject, GangMessageAudit, Task>> _handlers;
        readonly IGangSerializationService _serializer;

        public WebGangHost(
            IGangSerializationService serializer
            )
        {
            _connected = new TaskCompletionSource<bool>();
            _handlers = new Dictionary<string, Func<JObject, GangMessageAudit, Task>>{
                { "updateUser", (o, a)=> UpdateUser(o.ToObject<UpdateUserNameCommand>(), a) },
                { "addMessage", (o, a)=> AddMessage(o.ToObject<AddMessageCommand>(), a) }
            }.ToImmutableDictionary();
            _serializer = serializer;
        }

        public byte[] Id { get; } = Encoding.UTF8.GetBytes("HOST");

        IGangController _controller;
        async Task IGangMember.ConnectAndBlockAsync(
            IGangController controller)
        {
            _controller = controller;
            await _connected.Task;
        }

        Task IGangMember.DisconnectAsync(string reason)
        {
            Disconnect();
            return Task.CompletedTask;
        }

        async Task IGangMember.SendAsync(
            GangMessageTypes type,
            byte[] data, byte[] memberId)
        {
            var message = Encoding.UTF8.GetString(data);
            var audit = new GangMessageAudit(memberId ?? Id);
            switch (type)
            {
                case GangMessageTypes.Command:
                    var wrapper = _serializer.Deserialize<CommandWrapper>(data);
                    await _handlers[wrapper.Type](wrapper.Command, audit);

                    break;
                case GangMessageTypes.Member:

                    break;
                case GangMessageTypes.Connect:
                    await UpdateUser(new UpdateUserIsOnlineCommand(
                        message, true
                        ), audit);

                    break;
                case GangMessageTypes.Disconnect:
                    await UpdateUser(new UpdateUserIsOnlineCommand(
                        message, false
                        ), audit);

                    break;
            }
        }

        WebGangHostState _state = new WebGangHostState(
          new List<WebGangUser>(),
          new List<WebGangMessage>());

        async Task SetState(WebGangHostState state)
        {
            if (!state.Users.Any())
            {
                Disconnect();
                return;
            }

            _state = state;

            await _controller.SendStateAsync(state);
        }

        async Task UpdateUser(UpdateUserNameCommand command, GangMessageAudit _)
        {
            var user = _state.Users.First(u => u.Id == command.Id);
            await SetState(
                new WebGangHostState(
                  _state.Users.Replace(user, user.Update(command)),
                  _state.Messages
                ));
        }

        async Task UpdateUser(UpdateUserIsOnlineCommand command, GangMessageAudit _)
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

                SendWelcome(Encoding.UTF8.GetBytes(user.Id));
            }
            else
                await SetState(
                    new WebGangHostState(
                      _state.Users.Replace(user, user.Update(command)),
                      _state.Messages
                    ));
        }

        async void SendWelcome(byte[] memberId)
        {
            await Task.Delay(5000);

            await _controller.SendStateAsync(new
            {
                PrivateMessages = new[] {
                            new WebGangMessage(
                                "Welcome", DateTimeOffset.Now, null,
                                "Hello and welcome")
                              }
            },
            new[] { memberId });
        }

        async Task AddMessage(AddMessageCommand command, GangMessageAudit audit)
        {
            await SetState(
                new WebGangHostState(
                  _state.Users,
                  _state.Messages.Add(
                    new WebGangMessage(
                      command.Id, DateTimeOffset.UtcNow,
                      Encoding.UTF8.GetString(audit.MemberId),
                      command.Text)
                    ).TakeLast(10)
                ));
        }

        void Disconnect()
        {
            if (!_connected.Task.IsCompleted)
                _connected.SetResult(true);
        }

        void IDisposable.Dispose()
        {
            Disconnect();
        }
    }
}
