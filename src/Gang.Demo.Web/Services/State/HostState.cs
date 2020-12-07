using Gang.Demo.Web.Services.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.Demo.Web.Services.State
{
    public sealed class HostState
    {
        public HostState(
            IEnumerable<User> users = null,
            IEnumerable<Message> messages = null)
        {
            if (users != null
                && users.GroupBy(u => u.Id).Any(g => g.Count() > 2))
                throw new Exception("Duplicate users");

            Users = users.ToImmutableListDefaultEmpty();
            Messages = messages.ToImmutableListDefaultEmpty();
        }

        public IImmutableList<User> Users { get; }
        public IImmutableList<Message> Messages { get; }

        public HostState Apply(UserCreated data)
        {
            var user = new User(
                data.UserId, data.Name, null
                );

            return new HostState(
                    Users.Add(user),
                    Messages
                );
        }

        public HostState Apply(UserNameUpdated data)
        {
            var user = Users.First(u => u.Id == data.UserId);

            return new HostState(
                    Users.Replace(user, user.SetName(data.Name)),
                    Messages
                );
        }

        public HostState Apply(MessageAdded data)
        {
            var message = new Message(
                data.Id, data.Text,
                data.ById, data.On);

            return new HostState(
                    Users,
                    Messages.Add(message)
                );
        }

        public HostState Apply(UserMessageAdded data)
        {
            var user = Users.First(u => u.Id == data.UserId);
            var message = new Message(
                data.Id, data.Text,
                null, data.On);

            return new HostState(
                    Users.Replace(user, user.AddMessage(message)),
                    Messages
                );
        }
    }
}
