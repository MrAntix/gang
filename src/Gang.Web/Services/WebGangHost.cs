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
        readonly IGangSerializationService _serialization;
        readonly IGangHandler _handler;
        readonly IImmutableDictionary<string, Func<JObject, GangMessageAudit, Task>> _handlers;

        public WebGangHost(
            IGangSerializationService serialization,
            IGangHandler handler)
        {
            _connected = new TaskCompletionSource<bool>();
            _serialization = serialization;
            _handler = handler;
            _handlers = new Dictionary<string, Func<JObject, GangMessageAudit, Task>>{
                { "updateUser", (o, a)=> UpdateUser(o.ToObject<UpdateUserNameCommand>(), a) },
                { "addMessage", (o, a)=> AddMessage(o.ToObject<AddMessageCommand>(), a) }
            }.ToImmutableDictionary();
        }

        public byte[] Id { get; } = Encoding.UTF8.GetBytes("HOST");

        string _gangId;
        GangMemberSendAsync _sendAsync;
        async Task IGangMember.ConnectAsync(
            string gangId,
            GangMemberSendAsync sendAsync)
        {
            _gangId = gangId;
            _sendAsync = sendAsync;
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
                    var wrapper = _serialization.Deserialize<CommandWrapper>(message);
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

            await _sendAsync(Encoding.UTF8.GetBytes(
                _serialization.Serialize(state)
            ));
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
                await SetState(
                    new WebGangHostState(
                  _state.Users.Add(new WebGangUser(command.Id, null, command.IsOnline)),
                  _state.Messages
                ));

            else
                await SetState(
                    new WebGangHostState(
                      _state.Users.Replace(user, user.Update(command)),
                      _state.Messages
                    ));
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
