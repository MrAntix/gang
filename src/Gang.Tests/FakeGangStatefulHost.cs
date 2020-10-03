using Gang.Commands;
using Gang.WebSockets.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gang.Tests
{
    public class FakeGangStatefulHost :
        GangStatefulHostBase<FakeGangStatefulHost.FakeState>
    {
        readonly GangCommandExecutor<FakeGangStatefulHost> _commands;

        public FakeGangStatefulHost(
            IEnumerable<Func<IGangCommandHandler<FakeGangStatefulHost>>> commandHandlerProviders = null) :
            base(FakeState.Initial)
        {
            _commands = this
                .CreateCommandExecutor(new WebSocketGangJsonSerializationService())
                .Register<IncrementCommand>("increment", Increment)
                .Register<DecrementCommandHandler>();

            if (commandHandlerProviders != null)
                _commands = _commands.Register(commandHandlerProviders);
        }

        protected override async Task OnCommandAsync(
            byte[] data, GangMessageAudit audit)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (audit is null) throw new ArgumentNullException(nameof(audit));

            await base.OnCommandAsync(data, audit);

            await _commands.ExecuteAsync(data, audit);
        }

        public async Task SetCount(int value, GangMessageAudit audit)
        {
            if (audit is null) throw new ArgumentNullException(nameof(audit));

            if (State.Count > 2) throw new Exception("Too Big");
            if (State.Count < 0) throw new Exception("Too Small");

            await RaiseStateEventAsync(
                new CountSetEvent(value),
                audit.MemberId, FakeState.Apply
            );
        }

        async Task Increment(IncrementCommand _, GangMessageAudit a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));

            await SetCount(State.Count + 1, a);
        }

        public sealed class DecrementCommandHandler :
            GangCommandHandlerBase<FakeGangStatefulHost, DecrementCommand>
        {
            public override string CommandTypeName => "decrement";

            protected override async Task HandleAsync(
                FakeGangStatefulHost host, DecrementCommand _, GangMessageAudit a)
            {
                if (host is null) throw new ArgumentNullException(nameof(host));
                if (a is null) throw new ArgumentNullException(nameof(a));

                await host.SetCount(host.State.Count - 1, a);
            }
        }

        public sealed class SetCommandHandler :
            GangCommandHandlerBase<FakeGangStatefulHost, SetCommand>
        {
            public override string CommandTypeName => "set";

            protected override async Task HandleAsync(
                FakeGangStatefulHost host, SetCommand command, GangMessageAudit a)
            {
                if (host is null) throw new ArgumentNullException(nameof(host));
                if (command is null) throw new ArgumentNullException(nameof(command));
                if (a is null) throw new ArgumentNullException(nameof(a));

                await host.SetCount(command.Value, a);
            }
        }

        public sealed class FakeState
        {
            public FakeState(
                int count)
            {
                Count = count;
            }

            public int Count { get; }

            public static FakeState Apply(CountSetEvent e)
            {
                if (e is null) throw new ArgumentNullException(nameof(e));

                return new FakeState(e.Value);
            }

            public static FakeState Initial { get; } = new FakeState(1);
        }

        public class IncrementCommand { }
        public class DecrementCommand { }
        public class SetCommand
        {
            public SetCommand(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        public class CountSetEvent
        {
            public CountSetEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
}
