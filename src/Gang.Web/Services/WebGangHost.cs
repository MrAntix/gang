using Gang.Commands;
using Gang.Contracts;
using Gang.Members;
using Gang.Web.Services.Commands;
using Gang.Web.Services.Events;
using Gang.Web.Services.State;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangHost : GangStatefulHostBase<WebGangHostState>
    {
        readonly IGangCommandExecutor<WebGangHost> _commandExecutor;

        public WebGangHost(
            IGangCommandExecutor<WebGangHost> commandExecutor
            )
        {
            _commandExecutor = commandExecutor;
        }

        protected override Task OnConnectAsync()
        {
            SetState(new WebGangHostState(
              new List<WebGangUser>(),
              new List<WebGangMessage>())
            );

            ApplyStateEvents(
                new[]{
                    new GangEvent(
                        new WebGangMessageAddedEvent("Welcome", "Gang Chat Started"),
                        new GangAudit(Controller.GangId, Id, 1, DateTimeOffset.Now)
                        )
                });

            return base.OnConnectAsync();
        }

        protected override async Task OnMemberConnectAsync(
            byte[] memberId)
        {
            var member = Controller.GetGang().MemberById(memberId);
            var user = State.Users.TryGetById(member.Auth.Id);
            if (user == null) return;

            await UpdateUser(
                user.Id,
                user.MemberIds.Add(memberId.GangToString()),
                new GangAudit(Controller.GangId, memberId)
                );

            await SendWelcome(memberId);
        }

        protected override async Task OnMemberDisconnectAsync(
            byte[] memberId)
        {
            var user = State.Users.TryGetByMemberId(memberId);
            if (user == null) return;

            await UpdateUser(
                user.Id,
                user.MemberIds.Remove(memberId.GangToString()),
                new GangAudit(Controller.GangId, memberId)
                );

            await PrivateMessage(
                $"@{user.Id} Left",
                GetMemberIds(memberId).ToArray()
                );
        }

        protected async override Task OnCommandAsync(
            byte[] bytes, GangAudit audit)
        {
            try
            {
                await base.OnCommandAsync(bytes, audit);

                await _commandExecutor
                    .ExecuteAsync(this, bytes, audit);
            }
            catch (Exception ex)
            {
                await NotifyAsync(
                    new Notify(
                        "error", ex.Message
                    ),
                    new[] { audit.MemberId },
                    audit.SequenceNumber
                );
            }
        }

        protected override async Task OnStateEventAsync(
            object e, GangAudit a)
        {
            Console.WriteLine(
                $"EVENT: {e.GetType().Name}" +
                $"\n{JsonConvert.SerializeObject(e)}" +
                $"\n{JsonConvert.SerializeObject(a)}");

            var memberIds = GetMemberIds();

            await Controller.SendStateAsync(State, memberIds);
        }

        public async Task UpdateUser(
            string userId,
            IEnumerable<string> memberIds,
            GangAudit audit)
        {
            if (memberIds is null)
                throw new ArgumentNullException(nameof(memberIds));

            var e = new WebGangUserMemberIdsUpdatedEvent(
                    userId,
                    memberIds
            );

            await RaiseStateEventAsync(e, audit.MemberId, State.Apply);
        }

        public async Task UpdateUser(
            string name,
            GangAudit audit)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var member = Controller.GetGang().MemberById(audit.MemberId);
            var user = State.Users.TryGetById(member.Auth.Id);

            if (user != null)
            {
                var e = new WebGangUserNameUpdatedEvent(
                    user.Id,
                    name
                );

                await RaiseStateEventAsync(e, audit.MemberId, State.Apply);
            }
            else
            {
                var e = new WebGangUserCreatedEvent(
                    member.Auth.Id,
                    name,
                    member.Id, member.Auth.Roles
                );

                await RaiseStateEventAsync(e, audit.MemberId, State.Apply);

                await SendWelcome(audit.MemberId);
            }
        }

        public async Task AddMessage(
            string message,
            string messageId = null,
            GangAudit audit = null)
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
            var member = Controller.GetGang().MemberById(memberId);
            var user = State.Users.TryGetById(member.Auth.Id);

            if (user == null)
            {

                await PrivateMessage($"Hello enter your name to join in", memberId);
            }
            else
            {
                await PrivateMessage($"Hello @{user.Id}, welcome to the gang", memberId);

                await PrivateMessage($"@{user.Id} joined", GetMemberIds(memberId));
            }
        }

        public async Task NotifyAsync(
            Notify command,
            byte[][] memberIds,
            uint? inReplyToSequenceNumber
            )
        {
            await Controller.SendCommandAsync(
                typeof(Notify).GetCommandTypeName(),
                command,
                memberIds,
                inReplyToSequenceNumber
                );
        }

        byte[][] GetMemberIds(params byte[][] exceptMemberIds)
        {
            return Controller.GetGang().OtherMembers
                .Select(m => m.Id)
                .Where(mId => !exceptMemberIds.Any(eMId => eMId.SequenceEqual(mId)))
                .ToArray();
        }
    }
}
