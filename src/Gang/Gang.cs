using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang
{
    public class Gang
    {
        public Gang(
            IGangMember host,
            IEnumerable<IGangMember> clients = null)
        {
            Host = host;
            Clients = clients == null
                ? ImmutableArray<IGangMember>.Empty
                : clients.ToImmutableArray();
        }

        public IGangMember Host { get; }
        public IImmutableList<IGangMember> Clients { get; }

        public Gang AddClient(IGangMember client)
        {

            return new Gang(Host, Clients.Add(client));
        }

        public Gang RemoveClient(IGangMember client)
        {
            if (client == Host)
            {
                return Clients.Any()
                    ? new Gang(Clients[0], Clients.Skip(1))
                    : null;
            }

            return new Gang(Host, Clients.Remove(client));
        }

    }
}
