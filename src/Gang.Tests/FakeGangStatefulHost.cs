using Gang.Commands;
using Gang.WebSockets.Serialization;
using System;
using System.Threading.Tasks;

namespace Gang.Tests
{
    public class FakeGangStatefulHost :
        GangStatefulHostBase<FakeGangStatefulHost.FakeState>
    {
        readonly IGangCommandExecutor _executor
             = new GangCommandExecutor(new WebSocketGangJsonSerializationService());

        public FakeGangStatefulHost() :
            base(FakeState.Initial)
        {
            _executor = _executor
                .Register<IncrementCommand>("increment", Increment);
        }

        async Task Increment(IncrementCommand command, GangMessageAudit a)
        {
            if (State.Count > 1) throw new Exception("Too Big");

            await RaiseStateEventAsync(
                new IncrementedEvent(),
                a.MemberId, State.Apply
                );
        }

        public sealed class FakeState
        {
            public FakeState(
                int count)
            {
                Count = count;
            }

            public int Count { get; }

            public FakeState Apply(IncrementedEvent _)
            {
                return new FakeState(Count + 1);
            }

            public static FakeState Initial { get; } = new FakeState(0);
        }

        public class IncrementCommand { }

        public class IncrementedEvent { }
    }
}
