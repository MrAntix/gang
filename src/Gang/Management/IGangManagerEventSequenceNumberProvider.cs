namespace Gang.Management
{
    public interface IGangManagerEventSequenceNumberProvider
    {
        uint Next();
    }

    public class GangManagerInMemoryEventSequenceNumberProvider :
        IGangManagerEventSequenceNumberProvider
    {
        uint _lastSequenceNumber;

        uint IGangManagerEventSequenceNumberProvider.Next()
        {
            return ++_lastSequenceNumber;
        }
    }
}