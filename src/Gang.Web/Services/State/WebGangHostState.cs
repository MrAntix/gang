using Gang.Contracts;
using Gang.Web.Services.Events;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public WebGangHostState Apply(WebGangUserCreatedEvent e)
        {
            var user = new WebGangUser(e.UserId, e.Name, true);

            return new WebGangHostState(
                    Users.Add(user),
                    Messages
                );
        }

        public WebGangHostState Apply(WebGangUserNameUpdatedEvent e)
        {
            var user = Users.First(u => u.Id == e.UserId);

            return new WebGangHostState(
                    Users.Replace(user, user.Update(e)),
                    Messages
                );
        }

        public WebGangHostState Apply(WebGangUserIsOnlineUpdatedEvent e)
        {
            var user = Users.First(u => u.Id == e.UserId);

            return new WebGangHostState(
                    Users.Replace(user, user.Update(e)),
                    Messages
                );
        }

        public WebGangHostState Apply(WebGangMessageAddedEvent e, GangMessageAudit a)
        {
            var message = new WebGangMessage(
                e.MessageId,
                a.On, a.MemberId.GangToString(),
                e.Text);

            return new WebGangHostState(
                    Users,
                    Messages.Add(message)
                );
        }
    }
}
