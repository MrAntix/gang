using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Web.Services.State
{
    public class WebGangHostState
    {
        public WebGangHostState(
            IEnumerable<WebGangUser> users)
        {
            Users = users?.ToImmutableList();
        }

        public IImmutableList<WebGangUser> Users { get; }
    }
}
