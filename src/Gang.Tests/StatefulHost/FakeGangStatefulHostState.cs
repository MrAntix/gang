using System;

namespace Gang.Tests.StatefulHost
{
    public sealed class FakeGangStatefulHostState
    {
        public FakeGangStatefulHostState(
            int count)
        {
            Count = count;
        }

        public int Count { get; }

        public static FakeGangStatefulHostState Apply(CountSetEvent e)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));

            return new FakeGangStatefulHostState(e.Value);
        }

        public static FakeGangStatefulHostState Initial { get; } = new FakeGangStatefulHostState(1);
    }
}
