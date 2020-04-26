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
    public class WebGangHost : GangHostMemberBase
    {
        readonly IImmutableDictionary<string, Func<JObject, GangMessageAudit, Task>> _handlers;
        readonly IGangSerializationService _serializer;

        public WebGangHost(
            IGangSerializationService serializer
            )
        {
            _handlers = new Dictionary<string, Func<JObject, GangMessageAudit, Task>>{
                { "updateUser", (o, _)=> UpdateUser(o.ToObject<UpdateUserNameCommand>()) },
                { "addMessage", (o, a)=> AddMessage(o.ToObject<AddMessageCommand>(), a) }
            }.ToImmutableDictionary();
            _serializer = serializer;
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
            var wrapper = _serializer.Deserialize<CommandWrapper>(data);
            await _handlers[wrapper.Type](wrapper.Command, audit);
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

        async Task UpdateUser(UpdateUserIsOnlineCommand command)
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

        async Task UpdateUser(UpdateUserNameCommand command)
        {
            var user = _state.Users.First(u => u.Id == command.Id);
            await SetState(
                new WebGangHostState(
                  _state.Users.Replace(user, user.Update(command)),
                  _state.Messages
                ));
        }

        async void SendWelcome(byte[] memberId)
        {
            await Task.Delay(5000);

            await Controller.SendStateAsync(new
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
    }
}
