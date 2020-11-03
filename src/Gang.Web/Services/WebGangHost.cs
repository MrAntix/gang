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
              new List<State.WebGangUser>(),
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
            var user = State.Users.TryGetByIdString(memberId);
            if (user == null) return;

            await UpdateUser(
                user.Id, true,
                new GangMessageAudit(memberId, null)
                );

            await SendWelcome(memberId);
        }

        protected override async Task OnMemberDisconnectAsync(byte[] memberId)
        {
            var user = State.Users.TryGetByIdString(memberId);
            if (user == null) return;

            await UpdateUser(
                user.Id, false,
                new GangMessageAudit(memberId, null)
                );

            await PrivateMessage(
                $"@{user.Id} Left",
                GetUserMemberIds(memberId).ToArray()
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

            var userMemberIds = GetUserMemberIds();

            await Controller.SendStateAsync(State, userMemberIds);
        }

        public async Task UpdateUser(
            string userId,
            bool isOnline,
            GangMessageAudit audit)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            if (State.Users.Any(u => u.Id == userId))
            {
                var e = new WebGangUserIsOnlineUpdatedEvent(
                    userId,
                    isOnline
                );

                await RaiseStateEventAsync(e, audit.MemberId, State.Apply);
            }
        }

        public async Task UpdateUser(
            string userId,
            string name,
            GangMessageAudit audit)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (State.Users.Any(u => u.Id == userId))
            {
                var e = new WebGangUserNameUpdatedEvent(
                    userId,
                    name
                );

                await RaiseStateEventAsync(e, audit.MemberId, State.Apply);
            }
            else
            {
                var e = new WebGangUserCreatedEvent(
                    userId,
                    name
                );

                await RaiseStateEventAsync(e, audit.MemberId, State.Apply);

                await SendWelcome(audit.MemberId);
            }
        }

        public async Task AddMessage(
            string message,
            string messageId = null,
            GangMessageAudit audit = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            var e = new WebGangMessageAddedEvent(
                messageId ?? Guid.NewGuid().ToString("N"),
                message
            );

            await RaiseStateEventAsync(e, audit?.MemberId ?? Id, State.Apply);
        }

        public async Task PrivateMessage(
            string message,
            params byte[][] memberIds)
        {
            await Controller.SendStateAsync(new
            {
                PrivateMessages = new[] {
                            new WebGangMessage(
                                Guid.NewGuid().ToString("N"),
                                DateTimeOffset.Now, Id.GangToString(),
                                message)
                              }
            },
            memberIds);
        }

        public async Task SendWelcome(byte[] memberId)
        {
            var user = State.Users.TryGetByIdString(memberId);
            if (user == null)
            {

                await PrivateMessage($"Hello enter your name to join in", memberId);
            }
            else
            {
                await PrivateMessage($"Hello @{user.Id}, welcome to the gang", memberId);

                await PrivateMessage(
                    $"@{user.Id} joined",
                    GetUserMemberIds(memberId)
                    );
            }
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

        byte[][] GetUserMemberIds(params byte[][] exceptMemberIds)
        {
            var exceptUserIds = exceptMemberIds.Select(e => e.GangToString()).ToArray();

            return GetUserMemberIds(exceptUserIds);
        }

        byte[][] GetUserMemberIds(string[] exceptUserIds)
        {
            return State.Users
                .Where(u => !exceptUserIds.Contains(u.Id))
                .Select(u => u.Id.GangToBytes())
                .ToArray();
        }
    }
}
