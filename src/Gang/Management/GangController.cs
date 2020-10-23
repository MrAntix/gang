using Gang.Contracts;
using Gang.Members;
using Gang.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Management
{
    public sealed class GangController : IGangController
    {
        readonly string _gangId;
        readonly IGangManager _manager;
        readonly GangMemberSendAsync _sendAsync;
        readonly IGangSerializationService _serializer;
        uint _commandSequenceNumber = uint.MaxValue - 10;

        public GangController(
            string gangId,
            IGangManager manager,
            GangMemberSendAsync sendAsync,
            IGangSerializationService serializer
            )
        {
            _gangId = gangId;
            _manager = manager;
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

        async Task IGangController.SendAsync(
            byte[] data, GangMessageTypes? type, IEnumerable<byte[]> memberIds)
        {
            await _sendAsync(data, type, memberIds);
        }

        async Task IGangController.SendCommandAsync(
            string type, object command, IEnumerable<byte[]> memberIds, uint? inReplyToSequenceNumber)
        {
            var data = BitConverter.GetBytes(++_commandSequenceNumber)
                    .Concat(_serializer.Serialize(new
                    {
                        type,
                        command,
                        rsn = inReplyToSequenceNumber
                    }))
                    .ToArray();

            await _sendAsync(data, GangMessageTypes.Command, memberIds);
        }

        async Task IGangController.SendStateAsync<T>(
            T state, IEnumerable<byte[]> memberIds)
        {
            await _sendAsync(
                 _serializer.Serialize(state),
                 GangMessageTypes.State,
                 memberIds);
        }
    }
}