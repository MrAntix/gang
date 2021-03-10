using Gang.State;
using Gang.State.Commands;
using Gang.Tests.State.Todos;
using Xunit;

namespace Gang.Tests.State
{
    public sealed class GangStateResultTests
    {
        const string MEMBER_ID = "MEMBER_ID";
        const string NOTIFICATION_ID = "NOTIFICATION_ID";

        [Fact]
        public void results()
        {
            var state = GangState
                .Create(TodosState.Initial)
                .AddResult(MEMBER_ID.GangToBytes(),
                    new GangNotify(id: NOTIFICATION_ID)
                    );

            Assert.NotEmpty(state.Results);
        }
    }
}
