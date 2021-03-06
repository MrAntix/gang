using Gang.State;
using Gang.State.Commands;
using Gang.Tests.State.Todos;
using Xunit;

namespace Gang.Tests.State
{
    public sealed class GangStateNotificationTests
    {
        const string MEMBER_ID = "MEMBER_ID";
        const string NOTIFICATION_ID = "NOTIFICATION_ID";

        [Fact]
        public void notifications()
        {
            var state = GangState
                .Create(TodosState.Initial)
                .RaiseNotification(MEMBER_ID.GangToBytes(),
                    new GangNotify("Hey", id: NOTIFICATION_ID)
                    );

            Assert.NotEmpty(state.Notifications);
        }
    }
}
