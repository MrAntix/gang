using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gang.Web.Services.State
{
  public class WebGangHostState
  {
    public WebGangHostState(
        IEnumerable<WebGangUser> users,
        IEnumerable<WebGangMessage> messages)
    {
      Users = users?.ToImmutableList();
      Messages = messages?.ToImmutableList();
    }

    public IImmutableList<WebGangUser> Users { get; }
    public IImmutableList<WebGangMessage> Messages { get; }
  }
}
