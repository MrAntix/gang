using Gang.Commands;
using Gang.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang.Management
{
    public sealed class GangController : IGangController
    {
        readonly string _gangId;
        readonly IGangManager _manager;
        readonly GangMemberReceiveAsync _receiveAsync;
        readonly GangMemberSendAsync _sendAsync;
        readonly IGangSerializationService _serializer;
        uint _commandSequence = 0;

        public GangController(
            IGangManager manager,
            string gangId, IGangMember member,
            GangMemberReceiveAsync receiveAsync,
            GangMemberSendAsync sendAsync,
            IGangSerializationService serializer
            )
        {
            _gangId = gangId;
            Member = member;
            _manager = manager;
            _receiveAsync = receiveAsync;
            _sendAsync = sendAsync;
            _serializer = serializer;
        }

        string IGangController.GangId => _gangId;

        public IGangMember Member { get; }

        GangMemberCollection IGangController.GetGang()
        {
            return _manager.GangById(_gangId);
        }

        async Task IGangController.DisconnectAsync(
            byte[] memberId, string reason)
        {
            var gang = _manager.GangById(_gangId);
            var member = gang.MemberById(memberId);

            await member.DisconnectAsync(reason);
        }

        async Task IGangController.ReceiveAsync(
            byte[] data)
        {
            await _receiveAsync(data);
        }

        async Task IGangController.SendAsync(
            GangMessageTypes? type, byte[] data, IEnumerable<byte[]> memberIds)
        {
            var audit = new GangAudit(_gangId, null, Member.Id, Member.Session?.User.Id);
            await _sendAsync(type, data, audit, memberIds);
        }

        async Task IGangController.SendCommandAsync(
            string type, object data, IEnumerable<byte[]> memberIds, uint? replySequence)
        {
            var bytes = _serializer.SerializeCommandData(
                type, data,
                replySequence
                );
            var audit = new GangAudit(_gangId, ++_commandSequence, Member.Id, Member.Session?.User.Id);

            await _sendAsync(GangMessageTypes.Command, bytes, audit, memberIds);
        }

        async Task IGangController.SendStateAsync<T>(
            T state, IEnumerable<byte[]> memberIds)
        {
            var audit = new GangAudit(_gangId);

            await _sendAsync(
                 GangMessageTypes.State,
                 _serializer.Serialize(state),
                 audit,
                 memberIds);
        }
    }
}