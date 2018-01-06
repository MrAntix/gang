using System;

namespace Gang.Contracts
{
    public class GangParameters
    {
        public GangParameters(
            Guid gangId)
        {
            GangId = gangId;
        }

        public Guid GangId { get; }
    }
}
