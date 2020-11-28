namespace Gang.Management.Events
{
    public sealed class GangManagerInMemorySequenceProvider :
        IGangManagerSequenceProvider
    {
        uint _last;

        uint IGangManagerSequenceProvider.Next()
        {
            return ++_last;
        }
    }
}