using Gang.Commands;
using Gang.Events;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.Tests.StatefulHost
{
    public class FakeGangStatefulHost :
        GangStatefulHostBase<FakeGangStatefulHostState>
    {
        readonly IGangCommandExecutor<FakeGangStatefulHost> _commandExecutor;

        public FakeGangStatefulHost(
            IGangCommandExecutor<FakeGangStatefulHost> commandExecutor) :
            base(FakeGangStatefulHostState.Initial)
        {
            _commandExecutor = commandExecutor
                .RegisterHandler<Increment>(IncrementCommandHandler);
        }

        protected override async Task OnCommandAsync(
            byte[] bytes, GangAudit audit)
        {
            await base.OnCommandAsync(bytes, audit);

            await _commandExecutor.ExecuteAsync(this, bytes, audit);
        }

        protected override async Task OnEventAsync(
            object data, GangAudit a)
        {
            await base.OnEventAsync(data, a);

            Events = Events.Add(GangEvent.From(data, a));
        }

        public IImmutableList<IGangEvent> Events { get; private set; } = ImmutableList<IGangEvent>.Empty;

        public async Task SetCount(int value, GangAudit audit)
        {
            if (audit is null) throw new ArgumentNullException(nameof(audit));

            if (State.Count > 2) throw new Exception("Too Big");
            if (State.Count < 0) throw new Exception("Too Small");

            await RaiseStateEventAsync(
                new CountSet(value),
                audit, FakeGangStatefulHostState.Apply
            );
        }

        async Task IncrementCommandHandler(Increment _, GangAudit a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));

            await SetCount(State.Count + 1, a);
        }
    }
}
