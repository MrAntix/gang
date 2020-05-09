using Gang.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang
{
    public sealed class GangController : IGangController
    {
        readonly string _gangId;
        readonly IGangHandler _handler;
        readonly GangMemberSendAsync _sendAsync;
        readonly IGangSerializationService _serializer;

        public GangController(
            string gangId,
            IGangHandler handler,
            GangMemberSendAsync sendAsync,
            IGangSerializationService serializer
            )
        {
            _gangId = gangId;
            _handler = handler;
            _sendAsync = sendAsync;
            _serializer = serializer;
        }

        string IGangController.GangId => _gangId;

        GangMemberCollection IGangController.GetGang()
        {
            return _handler.GangById(_gangId);
        }

        async Task IGangController.DisconnectAsync(
            byte[] memberId, string reason)
        {
            var gang = _handler.GangById(_gangId);
            var member = gang.MemberById(memberId);

            await member.DisconnectAsync(reason);
        }

        async Task IGangController.SendAsync(
            byte[] data, GangMessageTypes? type, IEnumerable<byte[]> memberIds)
        {
            await _sendAsync(data, type, memberIds);
        }

        async Task IGangController.SendCommandAsync(
            IGangCommandWrapper wrapper, IEnumerable<byte[]> memberIds)
        {
            await _sendAsync(
                 _serializer.Serialize(new
                 {
                     type = wrapper.Type,
                     command = wrapper.Command
                 }),
                 GangMessageTypes.Command,
                 memberIds);
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