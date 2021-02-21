using Gang.State;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gang.Demo.Web.Server.State
{

    public sealed class User : IHasGangIdString
    {
        public User(
            string id,
            string name = null,
            IEnumerable<Message> messages = null
            )
        {
            UserId = id ?? throw new ArgumentNullException(nameof(id));
            Name = name;
            Messages = messages.ToImmutableListDefaultEmpty();
        }

        public string UserId { get; }
        public string Name { get; }
        public IImmutableList<Message> Messages { get; }

        public User SetName(string name)
        {
            return new User(
                UserId,
                name,
                Messages
                );
        }

        internal User AddMessage(Message message)
        {
            return new User(
                UserId,
                Name,
                Messages.Add(message)
                );
        }

        public const string ERROR_USER_NOT_FOUND = "User not found";
        public const string ERROR_USER_EXIST = "User already exists";
        public const string ERROR_NAME_IS_NOT_VALID = "Name is not valid";
        public const string ERROR_NAME_IS_TAKEN = "Name is taken";

        public static string Exists(
             GangState<HostState> state, string id)
        {
            return state.Data.Users.All(u => u.UserId != id)
                ? ERROR_USER_NOT_FOUND
                : null;
        }

        public static string DoesNotExist(
              GangState<HostState> state, string id)
        {
            return state.Data.Users.Any(u => u.UserId == id)
                ? ERROR_USER_EXIST
                : null;
        }

        public static string NameIsValid(string name)
        {
            return string.IsNullOrWhiteSpace(name)
                ? ERROR_NAME_IS_NOT_VALID
                : null;
        }

        public static string NameIsUnique(
            GangState<HostState> state, string id, string name)
        {
            return state.Data
                .Users.Any(u => u.UserId != id && u.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                ? ERROR_NAME_IS_TAKEN
                : null;
        }
    }
}
