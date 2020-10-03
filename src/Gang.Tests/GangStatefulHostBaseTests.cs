using Gang.Commands;
using Gang.Contracts;
using Gang.Events;
using Gang.WebSockets.Serialization;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests
{
    public sealed class GangStatefulHostBaseTests
    {
        readonly GangParameters _gangParameters = new GangParameters("gangId", null);

        [Fact]
        public async Task apply_events()
        {
            var host = GetHost();

            var handler = GetGangHander();
            await handler.HandleAsync(_gangParameters, host);

            var events = new[] {
                new GangStateEventWrapper(
                    new FakeGangStatefulHost.CountSetEvent(1),
                    new GangStateEventAudit(Array.Empty<byte>(), 10, DateTimeOffset.Now)
                    )
            };

            host.ApplyStateEvents(events);

            Assert.Equal((uint)10, host.StateVersion);
        }

        [Fact]
        public async Task apply_events_out_of_sequence()
        {
            var host = GetHost();

            var handler = GetGangHander();
            await handler.HandleAsync(_gangParameters, host);

            var events = new[] {
                new GangStateEventWrapper(
                    new FakeGangStatefulHost.CountSetEvent(0),
                    new GangStateEventAudit(Array.Empty<byte>(), 10, DateTimeOffset.Now)
                    ),
                new GangStateEventWrapper(
                    new FakeGangStatefulHost.CountSetEvent(1),
                    new GangStateEventAudit(Array.Empty<byte>(), 1, DateTimeOffset.Now)
                    )
            };

            Assert.Throws<GangStateOutOfSequenceException>(
                () => host.ApplyStateEvents(events)
                );
        }

        [Fact]
        public async Task send_command_with_method_handler()
        {
            var host = GetHost();

            var handler = GetGangHander();
            await handler.HandleAsync(_gangParameters, host);

            var member = new FakeGangMember("Member");
            await handler.HandleAsync(_gangParameters, member);

            await member.Controller.SendCommandAsync(
                "increment",
                new FakeGangStatefulHost.IncrementCommand());

            Assert.Equal(2, host.State.Count);
        }

        [Fact]
        public async Task send_command_with_class_handler()
        {
            var host = GetHost();

            var handler = GetGangHander();
            await handler.HandleAsync(_gangParameters, host);

            var member = new FakeGangMember("Member");
            await handler.HandleAsync(_gangParameters, member);

            await member.Controller.SendCommandAsync(
                "decrement",
                new FakeGangStatefulHost.DecrementCommand());

            Assert.Equal(0, host.State.Count);
        }

        [Fact]
        public async Task send_command_with_injected_class_handler()
        {
            var host = GetHost();

            var handler = GetGangHander();
            await handler.HandleAsync(_gangParameters, host);

            var member = new FakeGangMember("Member");
            await handler.HandleAsync(_gangParameters, member);

            await member.Controller.SendCommandAsync(
                "set",
                new FakeGangStatefulHost.SetCommand(1));

            Assert.Equal(1, host.State.Count);
        }

        static FakeGangStatefulHost GetHost()
        {
            var injectedCommandHandlers = new Func<IGangCommandHandler<FakeGangStatefulHost>>[]
            {
                () => new FakeGangStatefulHost.SetCommandHandler()
            };

            return new FakeGangStatefulHost(injectedCommandHandlers);
        }

        IGangHandler GetGangHander(
            GangCollection gangs = null)
        {
            return new GangHandler(
                gangs ?? new GangCollection(),
                new WebSocketGangJsonSerializationService()
                );
        }

    }
}
