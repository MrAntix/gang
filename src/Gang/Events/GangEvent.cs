using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Gang.Events
{
    public abstract class GangEvent
    {
        public GangEvent(
            string gangId
            )
        {
            GangId = gangId;
        }

        public string GangId { get; }
    }
}
