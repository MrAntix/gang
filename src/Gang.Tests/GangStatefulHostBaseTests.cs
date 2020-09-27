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
                    new FakeGangStatefulHost.IncrementedEvent(),
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
                    new FakeGangStatefulHost.IncrementedEvent(),
                    new GangStateEventAudit(Array.Empty<byte>(), 10, DateTimeOffset.Now)
                    ),
                new GangStateEventWrapper(
                    new FakeGangStatefulHost.IncrementedEvent(),
                    new GangStateEventAudit(Array.Empty<byte>(), 1, DateTimeOffset.Now)
                    )
            };

            Assert.Throws<GangStateOutOfSequenceException>(
                () => host.ApplyStateEvents(events)
                );
        }

        static FakeGangStatefulHost GetHost()
        {
            return new FakeGangStatefulHost();
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
