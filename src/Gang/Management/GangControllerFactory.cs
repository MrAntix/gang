using Gang.Serialization;

namespace Gang.Management
{
    public sealed class GangControllerFactory :
        IGangControllerFactory
    {
        readonly IGangSerializationService _serializer;

        public GangControllerFactory(
            IGangSerializationService serializer)
        {
            _serializer = serializer;
        }

        IGangController IGangControllerFactory
            .Create(
                IGangManager manager, string gangId, IGangMember member,
                GangMemberReceiveAsync receiveAsync, GangMemberSendAsync sendAsync
            )
        {
            return new GangController(
                    manager, gangId, member,
                    receiveAsync, sendAsync,
                    _serializer
                );
        }
    }
}