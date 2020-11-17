using Antix.Handlers;
using Gang.Commands;
using Gang.Contracts;
using Gang.Management;
using Gang.Members;
using Gang.Tests.StatefulHost;
using Gang.WebSockets.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests
{
    public sealed class GangStatefulHostBaseTests
    {
        readonly GangParameters _gangParameters = new("gangId", null);

        [Fact]
        public async Task apply_events()
        {
            var host = GetHost();

            var handler = GetGangHandler();
            await handler.ManageAsync(_gangParameters, host);

            var events = new[] {
                new GangEvent(
                    new CountSetEvent(1),
                    new GangAudit(_gangParameters.GangId, host.Id, 10, DateTimeOffset.Now)
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
                new GangEvent(
                    new CountSetEvent(0),
                    new GangAudit(_gangParameters.GangId, host.Id,  10, DateTimeOffset.Now)
                    ),
                new GangEvent(
                    new CountSetEvent(1),
                    new GangAudit(_gangParameters.GangId, host.Id, 1, DateTimeOffset.Now)
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

            await member.Controller
                .SendCommandAsync(new Increment());

            Assert.Equal(2, host.State.Count);
        }

        [Fact]
        public async Task send_command_with_provided_class_handler()
        {
            var host = GetHost();

            var handler = GetGangHandler();
            await handler.ManageAsync(_gangParameters, host);

            var member = new FakeGangMember("Member");
            await handler.ManageAsync(_gangParameters, member);

            await member.Controller
                .SendCommandAsync(new SetCount(1));

            Assert.Equal(1, host.State.Count);
        }

        static FakeGangStatefulHost GetHost()
        {
            var commandExecutor = GetGangCommandExecutor()
                .RegisterHandlerProvider(() => new DecrementHandler())
                .RegisterHandlerProvider(() => new SetHandler());

            return new FakeGangStatefulHost(commandExecutor);
        }

        static IGangCommandExecutor<FakeGangStatefulHost> GetGangCommandExecutor()
        {
            var serializer = new WebSocketGangJsonSerializationService();
            return new GangCommandExecutor<FakeGangStatefulHost>(
                serializer,
                new Executor<IGangCommand, FakeGangStatefulHost>());
        }

        static IGangManager GetGangHandler(
            GangCollection gangs = null)
        {
            return new GangManager(
                NullLogger<GangManager>.Instance,
                gangs ?? new GangCollection(),
                new WebSocketGangJsonSerializationService(),
                new GangManagerInMemoryEventSequenceNumberProvider()
                );
        }

    }
}
