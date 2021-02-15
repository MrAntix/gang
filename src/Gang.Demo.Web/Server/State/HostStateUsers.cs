using Gang.Demo.Web.Server.Events;
using Gang.State;

namespace Gang.Demo.Web.Server.State
{
    public static class HostStateUsers
    {
        public static GangState<HostState> CreateUser(
            this GangState<HostState> state,
            string id, string name)
        {
            return state
                    .Assert(User.NameIsValid(name))
                    .Assert(User.NameIsUnique(state, id, name))
                    .Assert(User.DoesNotExist(state, id))
                    .RaiseEvent(
                        new UserCreated(id, name),
                        state.Data.Apply
                    );
        }

        public static GangState<HostState> SetUserName(
            this GangState<HostState> state,
            string id, string name)
        {
            return state
                    .Assert(User.NameIsValid(name))
                    .Assert(User.NameIsUnique(state, id, name))
                    .Assert(User.Exists(state, id))
                    .RaiseEvent(
                        new UserNameUpdated(id, name),
                        state.Data.Apply
                    );
        }
    }
}
