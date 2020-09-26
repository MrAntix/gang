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

        public WebGangHostState Update(WebGangUserCreatedEvent e)
        {
            var user = new WebGangUser(e.UserId, null, e.IsOnline);

            return new WebGangHostState(
                                    Users.Add(user),
                                    Messages
                                );
        }

        public WebGangHostState Update(WebGangUserNameUpdatedEvent e)
        {
            var user = Users.First(u => u.Id == e.UserId);

            return new WebGangHostState(
                  Users.Replace(user, user.Update(e)),
                  Messages
                );
        }

        public WebGangHostState Update(WebGangUserIsOnlineUpdatedEvent e)
        {
            var user = Users.First(u => u.Id == e.UserId);

            return new WebGangHostState(
                  Users.Replace(user, user.Update(e)),
                  Messages
                );
        }

        public WebGangHostState Update(WebGangMessageAddedEvent e)
        {
            var message = new WebGangMessage(
                e.MessageId,
                e.Audit.On, e.Audit.MemberId.GangToString(),
                e.Text);

            return new WebGangHostState(
                                    Users,
                                    Messages.Add(message)
                                );
        }
    }
}
