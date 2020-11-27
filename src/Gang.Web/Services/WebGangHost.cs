using Gang.Commands;
using Gang.Management;
using Gang.State;
using Gang.State.Commands;
using Gang.State.Storage;
using Gang.Web.Services.Commands;
using Gang.Web.Services.State;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangHost :
        GangStateHost<WebGangAggregate, WebGangHostState>
    {
        public WebGangHost(
            IGangCommandExecutor<WebGangAggregate> executor,
            IGangAggregateStore<WebGangAggregate> store) :
            base(executor, store)
        {
        }

        protected override async Task OnMemberConnectAsync(
            byte[] memberId)
        {
            var member = Controller.GetMember(memberId);
            var user = State.Users.TryGetById(member.Auth.Id);
            if (user == null) return;

            //await UpdateUser(
            //    user.Id,
            //    user.MemberIds.Add(memberId.GangToString()),
            //    new GangAudit(Controller.GangId, null, memberId)
            //    );

            //await SendWelcome(memberId);
        }

        protected override async Task OnMemberDisconnectAsync(
            byte[] memberId)
        {
            //var user = State.Users.TryGetByMemberId(memberId);
            //if (user == null) return;

            //await UpdateUser(
            //    user.Id,
            //    user.MemberIds.Remove(memberId.GangToString()),
            //    new GangAudit(Controller.GangId, null, memberId)
            //    );

            //await PrivateMessage(
            //    $"@{user.Id} Left",
            //    GetMemberIds(memberId).ToArray()
            //    );
        }

        protected async override Task OnCommandAsync(
            byte[] bytes, GangAudit audit)
        {
            //try
            //{
            //    await base.OnCommandAsync(bytes, audit);

            //    await _commandExecutor
            //        .ExecuteAsync(this, bytes, audit);
            //}
            //catch (Exception ex)
            //{
            //    await NotifyAsync(
            //        new Notify(
            //            "error", ex.Message
            //        ),
            //        new[] { audit.MemberId },
            //        audit.SequenceNumber
            //    );
            //}
        }

        //protected override async Task OnEventAsync(
        //    object e, GangAudit a)
        //{
        //    Console.WriteLine(
        //        $"EVENT: {e.GetType().Name}" +
        //        $"\n{JsonConvert.SerializeObject(e)}" +
        //        $"\n{JsonConvert.SerializeObject(a)}");

        //    var memberIds = GetMemberIds();

        //    await Controller.SendStateAsync(State, memberIds);
        //}


        //public async Task SendWelcome(byte[] memberId)
        //{
        //    var member = Controller.GetMember(memberId);
        //    var user = State.Users.TryGetById(member.Auth.Id);

        //    if (user == null)
        //    {

        //        await PrivateMessage($"Hello enter your name to join in", memberId);
        //    }
        //    else
        //    {
        //        await PrivateMessage($"Hello @{user.Id}, welcome to the gang", memberId);

        //        await PrivateMessage($"@{user.Id} joined", GetMemberIds(memberId));
        //    }
        //}

        public async Task NotifyAsync(
            Notify command,
            byte[][] memberIds,
            uint? inReplyToSequenceNumber
            )
        {
            await Controller.SendCommandAsync(
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
