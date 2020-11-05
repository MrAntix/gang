using Gang.Contracts;
using Gang.Management;
using Gang.Management.Contracts;
using Gang.WebSockets.Serialization;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests
{
    public class GangManagerTests
    {
        readonly GangParameters _gangParameters = new("gangId", null);

        [Fact]
        public async Task first_connection_sent_host_message()
        {
            var handler = GetGangManager();
            var firstGangMember = new FakeGangMember("firstGangMember");

            await handler.ManageAsync(_gangParameters, firstGangMember);

            Assert.Equal(GangMessageTypes.Host, firstGangMember.MessagesReceived[0].Type);
        }

        [Fact]
        public async Task second_connection_sent_member_message()
        {
            var handler = GetGangManager();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            await handler.ManageAsync(_gangParameters, firstGangMember);
            await handler.ManageAsync(_gangParameters, secondGangMember);

            Assert.Equal(GangMessageTypes.Member, secondGangMember.MessagesReceived[0].Type);
        }

        [Fact]
        public async Task second_connection_sent_disconnect_message_when_first_closes()
        {
            var handler = GetGangManager();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            await handler.ManageAsync(_gangParameters, firstGangMember);
            await handler.ManageAsync(_gangParameters, secondGangMember);

            await firstGangMember.DisconnectAsync();

            Assert.Equal(GangMessageTypes.Disconnect, secondGangMember.MessagesReceived[1].Type);
        }

        [Fact]
        public async Task host_send_will_broadcast_state()
        {
            var handler = GetGangManager();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");
            var thirdGangMember = new FakeGangMember("thirdGangMember");

            await handler.ManageAsync(_gangParameters, firstGangMember);
            await handler.ManageAsync(_gangParameters, secondGangMember);
            await handler.ManageAsync(_gangParameters, thirdGangMember);

            await firstGangMember.Controller.SendAsync(new[] { (byte)1 });

            Assert.Equal(GangMessageTypes.State, secondGangMember.MessagesReceived[1].Type);
            Assert.Equal(GangMessageTypes.State, thirdGangMember.MessagesReceived[1].Type);
        }

        [Fact]
        public async Task member_send_will_send_command_to_host()
        {
            var handler = GetGangManager();

            using var hostMember = new FakeGangMember("hostMember");
            using var otherMember = new FakeGangMember("otherMember");

            await handler.ManageAsync(_gangParameters, hostMember);
            await handler.ManageAsync(_gangParameters, otherMember);

            uint sequenceNumber = 266;
            await otherMember.Controller.SendAsync(BitConverter.GetBytes(sequenceNumber));

            Assert.Equal(GangMessageTypes.Command, hostMember.MessagesReceived[2].Type);
            Assert.Equal(sequenceNumber, hostMember.MessagesReceived[2].SequenceNumber.Value);
        }

        [Fact]
        public async Task member_add_remove_events()
        {
            var handler = GetGangManager();
            var events = new List<IGangManagerEvent>();
            handler.Events.Subscribe(e => events.Add(e));

            var hostMember = new FakeGangMember("hostMember");

            await handler.ManageAsync(_gangParameters, hostMember);

            await hostMember.DisconnectAsync();

            Assert.Equal(3, events.Count);
            Assert.IsType<GangAdded>(events[0].Data);
            Assert.IsType<GangMemberAdded>(events[1].Data);
            Assert.IsType<GangMemberRemoved>(events[2].Data);
        }

        [Fact]
        public async Task add_host_member_on_gang_added_event()
        {
            var manager = GetGangManager();

            var hostMember = new FakeGangMember("host");
            var firstGangMember = new FakeGangMember("firstGangMember");

            using var _ = manager.Events
                .OfType<GangManagerEvent<GangAdded>>()
                .Subscribe(async e =>

                 await manager.ManageAsync(_gangParameters, hostMember).BlockAsync()
             );

            await manager.ManageAsync(_gangParameters, firstGangMember);

            var gang = manager.GangById(_gangParameters.GangId);
            Assert.Equal(2, gang.Members.Count);

            Assert.Equal(hostMember, gang.HostMember);

        }

        static IGangManager GetGangManager(
            GangCollection gangs = null)
        {
            return new GangManager(
                gangs ?? new GangCollection(),
                new WebSocketGangJsonSerializationService(),
                new GangManagerInMemoryEventSequenceNumberProvider()
                );
        }
    }
}
