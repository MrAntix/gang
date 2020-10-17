using Gang.Commands;
using Gang.Contracts;
using Gang.Events;
using Gang.Management;
using Gang.Members;
using Gang.Tests.StatefulHost;
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

            var handler = GetGangHandler();
            await handler.ManageAsync(_gangParameters, host);

            var events = new[] {
                new GangEventWrapper(
                    new CountSetEvent(1),
                    new GangMessageAudit(host.Id, Array.Empty<byte>(), 10, DateTimeOffset.Now)
                    )
            };

            host.ApplyStateEvents(events);

            Assert.Equal((uint)10, host.StateVersion);
        }

        [Fact]
        public async Task apply_events_out_of_sequence()
        {
            var host = GetHost();

            var handler = GetGangHandler();
            await handler.ManageAsync(_gangParameters, host);

            var events = new[] {
                new GangEventWrapper(
                    new CountSetEvent(0),
                    new GangMessageAudit(host.Id, Array.Empty<byte>(), 10, DateTimeOffset.Now)
                    ),
                new GangEventWrapper(
                    new CountSetEvent(1),
                    new GangMessageAudit(host.Id, Array.Empty<byte>(), 1, DateTimeOffset.Now)
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

            var handler = GetGangHandler();
            await handler.ManageAsync(_gangParameters, host);

            var member = new FakeGangMember("Member");
            await handler.ManageAsync(_gangParameters, member);

            await member.Controller.SendCommandAsync(
                "increment",
                new IncrementCommand());

            Assert.Equal(2, host.State.Count);
        }

        [Fact]
        public async Task send_command_with_class_handler()
        {
            var host = GetHost();

            var handler = GetGangHandler();
            await handler.ManageAsync(_gangParameters, host);

            var member = new FakeGangMember("Member");
            await handler.ManageAsync(_gangParameters, member);

            await member.Controller.SendCommandAsync(
                "decrement",
                new DecrementCommand());

            Assert.Equal(0, host.State.Count);
        }

        [Fact]
        public async Task send_command_with_injected_class_handler()
        {
            var host = GetHost();

            var handler = GetGangHandler();
            await handler.ManageAsync(_gangParameters, host);

            var member = new FakeGangMember("Member");
            await handler.ManageAsync(_gangParameters, member);

            await member.Controller.SendCommandAsync(
                "set",
                new SetCommand(1));

            Assert.Equal(1, host.State.Count);
        }

        static FakeGangStatefulHost GetHost()
        {
            var commandExecutor = GetGangCommandExecutor()
                .RegisterHandlerProvider(() => new SetCommandHandler());

            return new FakeGangStatefulHost(commandExecutor);
        }

        static IGangCommandExecutor<FakeGangStatefulHost> GetGangCommandExecutor()
        {
            var serializer = new WebSocketGangJsonSerializationService();
            return new GangCommandExecutor<FakeGangStatefulHost>(serializer);
        }

        static IGangManager GetGangHandler(
            GangCollection gangs = null)
        {
            return new GangManager(
                gangs ?? new GangCollection(),
                new WebSocketGangJsonSerializationService()
                );
        }

    }
}
