using Gang.Contracts;
using Gang.Management;
using Gang.Management.Events;
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
        readonly GangParameters _gangParameters = new GangParameters("gangId", null);

        [Fact]
        public async Task first_connection_sent_host_message()
        {
            var handler = GetGangHander();
            var firstGangMember = new FakeGangMember("firstGangMember");

            await handler.ManageAsync(_gangParameters, firstGangMember);

            Assert.Equal(GangMessageTypes.Host, firstGangMember.MessagesReceived[0].Type);
        }

        [Fact]
        public async Task second_connection_sent_member_message()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            await handler.ManageAsync(_gangParameters, firstGangMember);
            await handler.ManageAsync(_gangParameters, secondGangMember);

            Assert.Equal(GangMessageTypes.Member, secondGangMember.MessagesReceived[0].Type);
        }

        [Fact]
        public async Task second_connection_sent_disconnect_message_when_first_closes()
        {
            var handler = GetGangHander();

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
            var handler = GetGangHander();

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
            var handler = GetGangHander();

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
            var handler = GetGangHander();
            var events = new List<GangManagerEvent>();
            handler.Events.Subscribe(e => events.Add(e));

            var hostMember = new FakeGangMember("hostMember");

            await handler.ManageAsync(_gangParameters, hostMember);

            await hostMember.DisconnectAsync();

            Assert.Equal(3, events.Count);
            Assert.IsType<GangAddedManagerEvent>(events[0]);
            Assert.IsType<GangMemberAddedManagerEvent>(events[1]);
            Assert.IsType<GangMemberRemovedManagerEvent>(events[2]);
        }

        [Fact]
        public async Task add_host_member_on_gang_added_event()
        {
            var handler = GetGangHander();

            var hostMember = new FakeGangMember("host");
            var firstGangMember = new FakeGangMember("firstGangMember");

            using (handler.Events
                .OfType<GangAddedManagerEvent>()
                .Subscribe(async e =>

                 await handler.ManageAsync(_gangParameters, hostMember).BlockAsync()
             ))
            {
                await handler.ManageAsync(_gangParameters, firstGangMember);

                var gang = handler.GangById(_gangParameters.GangId);
                Assert.Equal(2, gang.Members.Count);

                Assert.Equal(hostMember, gang.HostMember);
            }
        }

        IGangManager GetGangHander(
            GangCollection gangs = null)
        {
            return new GangManager(
                gangs ?? new GangCollection(),
                new WebSocketGangJsonSerializationService()
                );
        }
    }
}
