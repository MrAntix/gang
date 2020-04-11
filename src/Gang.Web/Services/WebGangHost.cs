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
    readonly IImmutableDictionary<string, Func<JObject, GangMessageAudit, Task>> _handlers;

    public WebGangHost(
        ISerializationService serialization)
    {
      _connected = new TaskCompletionSource<bool>();
      _serialization = serialization;
      _handlers = new Dictionary<string, Func<JObject, GangMessageAudit, Task>>{
                { "updateUser", (o, a)=> UpdateUser(o.ToObject<UpdateUserCommand>(), a) },
                { "addMessage", (o, a)=> AddMessage(o.ToObject<AddMessageCommand>(), a) }
            }.ToImmutableDictionary();
    }

    public byte[] Id { get; } = Encoding.UTF8.GetBytes("Host");

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
          await AddUser(message, audit);

          break;
        case GangMessageTypes.Disconnect:
          await RemoveUser(message, audit);

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

      await _broadcastAsync(Encoding.UTF8.GetBytes(
          _serialization.Serialize(state)
      ));
    }

    async Task AddUser(string userId, GangMessageAudit audit)
    {
      await SetState(
          new WebGangHostState(
            _state.Users.Add(new WebGangUser(userId, null)),
            _state.Messages
          ));
    }

    async Task RemoveUser(string userId, GangMessageAudit audit)
    {
      var user = _state.Users.First(u => u.Id == userId);
      await SetState(
          new WebGangHostState(
            _state.Users.Remove(user),
            _state.Messages
          ));
    }

    async Task UpdateUser(UpdateUserCommand command, GangMessageAudit audit)
    {
      var user = _state.Users.First(u => u.Id == command.Id);
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
