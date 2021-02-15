using Gang.Demo.Web.Server.Events;
using Gang.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gang.Demo.Web.Server.State
{
    public static class HostStateMessages
    {
        public static GangState<HostState> AddMessage(
            this GangState<HostState> state,
            string id, string text,
            string byId, DateTimeOffset on
            )
        {
            id = Message.EnsureId(id);

            return state
                .Assert(User.Exists(state, byId))
                .Assert(Message.TextIsRequred(text))
                .RaiseEvent(
                   new MessageAdded(
                       id, text,
                       byId, on),
                   state.Data.Apply
                );
        }

        public static GangState<HostState> AddUserMessage(
            this GangState<HostState> state,
            string id, string text,
            IEnumerable<string> userIds
            )
        {
            id = Message.EnsureId(id);

            return userIds.Aggregate(
                state
                    .Assert(Message.TextIsRequred(text)),
                (s, userId) => s
                    .Assert(User.Exists(s, userId))
                    .RaiseEvent(
                        new UserMessageAdded(
                            userId,
                            id, text,
                            DateTimeOffset.Now),
                        s.Data.Apply
                    )
            );
        }

        public static GangState<HostState> AddUserMessage(
            this GangState<HostState> state,
            string text,
            IEnumerable<string> userIds
            )
        {
            return AddUserMessage(state, null, text, userIds);
        }
    }
}
