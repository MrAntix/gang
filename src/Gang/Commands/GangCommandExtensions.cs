using Gang.Serialization;

namespace Gang.Commands
{
    public static class GangCommandExtensions
    {
        public static GangCommandExecutor<THost> CreateCommandExecutor<THost>(
            this THost host,
            IGangSerializationService serializer)
        {
            return new GangCommandExecutor<THost>(host, serializer);
        }
    }

}