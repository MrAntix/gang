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
        readonly ISerializationService _serialization;
        readonly IImmutableDictionary<string, Func<JObject, Task>> _handlers;

        public WebGangHost(
            ISerializationService serialization)
        {
            _connected = new TaskCompletionSource<bool>();
            _serialization = serialization;
            _handlers = new Dictionary<string, Func<JObject, Task>>{
                { "updateUser", o=> UpdateUser(o.ToObject<UpdateUserCommand>()) }
            }.ToImmutableDictionary();
        }

        byte[] IGangMember.Id => Encoding.UTF8.GetBytes("Host");

        Func<byte[], Task> _broadcastAsync;
        async Task IGangMember.ConnectAsync(Func<byte[], Task> broadcastAsync)
        {
            _broadcastAsync = broadcastAsync;
            await _connected.Task;
        }

        Task IGangMember.DisconnectAsync(string reason)
        {
            Disconnect();
            return Task.CompletedTask;
        }

        async Task IGangMember.SendAsync(
            GangMessageTypes type, byte[] data)
        {
            var message = Encoding.UTF8.GetString(data);
            switch (type)
            {
                case GangMessageTypes.Command:
                    var wrapper = _serialization.Deserialize<CommandWrapper>(message);
                    await _handlers[wrapper.Type](wrapper.Command);

                    break;
                case GangMessageTypes.Member:

                    break;
                case GangMessageTypes.Connect:
                    await AddUser(message);

                    break;
                case GangMessageTypes.Disconnect:
                    await RemoveUser(message);

                    break;
            }
        }

        WebGangHostState _state = new WebGangHostState(new List<WebGangUser>());

        async Task SetState(WebGangHostState state)
        {
            if (!state.Users.Any())
            {
                Disconnect();
                return;
            }

            _state = state;

            await _broadcastAsync(Encoding.UTF8.GetBytes(
                _serialization.Serialize(state)
            ));
        }

        async Task AddUser(string userId)
        {
            await SetState(
                new WebGangHostState(
                _state.Users.Add(new WebGangUser(userId, "Anon"))
                ));
        }

        async Task RemoveUser(string userId)
        {
            var user = _state.Users.First(u => u.Id == userId);
            await SetState(
                new WebGangHostState(
                _state.Users.Remove(user)
                ));
        }

        async Task UpdateUser(UpdateUserCommand command)
        {
            var user = _state.Users.First(u => u.Id == command.Id);
            await SetState(
                new WebGangHostState(
                _state.Users.Replace(user, user.Update(command))
                ));
        }

        void Disconnect()
        {
            _connected.SetResult(true);
        }

        void IDisposable.Dispose()
        {
            Disconnect();
        }
    }
}
