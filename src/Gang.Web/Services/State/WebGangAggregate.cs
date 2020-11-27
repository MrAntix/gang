using Gang.State;
using Gang.Web.Services.Events;
using System;
using System.Collections.Generic;

namespace Gang.Web.Services.State
{
    public class WebGangAggregate :
        GangAggregateBase<WebGangAggregate, WebGangHostState>
    {
        public WebGangAggregate(
            WebGangHostState state = null,
            uint version = 0, IEnumerable<object> uncommitted = null) :
            base(
                (s, v, u) => new WebGangAggregate(s, v, u),
                state, version, uncommitted)
        {
        }

        public WebGangAggregate SetUserName(
            string id, string name)
        {
            var user = State.Users.TryGetById(id);
            if (user == null)
            {
                return Raise(
                    new WebGangUserCreated(id, name),
                    State.Apply
                    );
            }
            else
            {
                return Raise(
                    new WebGangUserNameUpdated(id, name),
                    State.Apply
                    );
            }
        }

        public WebGangAggregate AddMessage(
            string id, string text,
            string userId
            )
        {
            var user = State.Users.TryGetById(userId);
            if (user == null) throw new Exception("user not found");

            return Raise(
                new WebGangMessageAdded(
                    id, text,
                    userId, DateTimeOffset.Now),
                State.Apply
                );
        }
    }
}
