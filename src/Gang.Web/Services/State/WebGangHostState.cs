using Gang.Contracts;
using Gang.Web.Services.Events;
using System;
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
            if (users != null
                && users.GroupBy(u => u.Id).Any(g => g.Count() > 2))
                throw new Exception("Duplicate users");

            Users = users?.ToImmutableList();
            Messages = messages?.ToImmutableList();
        }

        public IImmutableList<WebGangUser> Users { get; }
        public IImmutableList<WebGangMessage> Messages { get; }

        public WebGangHostState Apply(WebGangUserCreated e)
        {
            var user = new WebGangUser(
                e.UserId, e.Name,
                new[] { e.MemberId.GangToString() },
                e.Properties);

            return new WebGangHostState(
                    Users.Add(user),
                    Messages
                );
        }

        public WebGangHostState Apply(WebGangUserNameUpdated e)
        {
            var user = Users.First(u => u.Id == e.UserId);

            return new WebGangHostState(
                    Users.Replace(user, user.Update(e)),
                    Messages
                );
        }

        public WebGangHostState Apply(WebGangUserMemberIdsUpdated e)
        {
            var user = Users.First(u => u.Id == e.UserId);

            return new WebGangHostState(
                    Users.Replace(user, user.Update(e)),
                    Messages
                );
        }

        public WebGangHostState Apply(WebGangMessageAdded e, GangAudit a)
        {
            var user = Users.TryGetByMemberId(a.MemberId);

            var message = new WebGangMessage(
                e.MessageId,
                a.On, user?.Id,
                e.Text);

            return new WebGangHostState(
                    Users,
                    Messages.Add(message)
                );
        }
    }
}
