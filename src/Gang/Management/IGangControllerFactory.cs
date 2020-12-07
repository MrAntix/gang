namespace Gang.Management
{
    public interface IGangControllerFactory
    {
        IGangController Create(
            IGangManager manager,
            string gangId, IGangMember member,
            GangMemberReceiveAsync receiveAsync,
            GangMemberSendAsync sendAsync
            );
    }
}