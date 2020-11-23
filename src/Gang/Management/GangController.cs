using Gang.Contracts;
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
        uint _commandSequenceNumber = uint.MaxValue - 10;

        public GangController(
            string gangId,
            IGangManager manager,
            GangMemberReceiveAsync receiveAsync,
            GangMemberSendAsync sendAsync,
            IGangSerializationService serializer
            )
        {
            _gangId = gangId;
            _manager = manager;
            _receiveAsync = receiveAsync;
            _sendAsync = sendAsync;
            _serializer = serializer;
        }

        string IGangController.GangId => _gangId;

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
            await _sendAsync(type, data, memberIds);
        }

        async Task IGangController.SendCommandAsync(
            string type, object data, IEnumerable<byte[]> memberIds, uint? replySequenceNumber)
        {
            var bytes = _serializer.SerializeCommand(
                ++_commandSequenceNumber,
                type, data,
                replySequenceNumber
                );

            await _sendAsync(GangMessageTypes.Command, bytes, memberIds);
        }

        async Task IGangController.SendStateAsync<T>(
            T state, IEnumerable<byte[]> memberIds)
        {
            await _sendAsync(
                 GangMessageTypes.State,
                 _serializer.Serialize(state),
                 memberIds);
        }
    }
}