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
            IEnumerable<WebGangUser> users = null,
            IEnumerable<WebGangMessage> messages = null)
        {
            if (users != null
                && users.GroupBy(u => u.Id).Any(g => g.Count() > 2))
                throw new Exception("Duplicate users");

            Users = users?.ToImmutableList();
            Messages = messages?.ToImmutableList();
        }

        public WebGangHostState() : this(null, null) { }

        public IImmutableList<WebGangUser> Users { get; }
        public IImmutableList<WebGangMessage> Messages { get; }

        public WebGangHostState Apply(WebGangUserCreated e)
        {
            var user = new WebGangUser(
                e.UserId, e.Name
                );

            return new WebGangHostState(
                    Users.Add(user),
                    Messages
                );
        }

        public WebGangHostState Apply(WebGangUserNameUpdated data)
        {
            var user = Users.First(u => u.Id == data.UserId);

            return new WebGangHostState(
                    Users.Replace(user, user.SetName(data.Name)),
                    Messages
                );
        }

        public WebGangHostState Apply(WebGangMessageAdded data)
        {

            var message = new WebGangMessage(
                data.Id, data.Text,
                data.UserId, data.On);

            return new WebGangHostState(
                    Users,
                    Messages.Add(message)
                );
        }
    }
}
