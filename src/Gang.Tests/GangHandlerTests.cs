using Gang.Contracts;
using Gang.Events;
using Gang.WebSockets.Serialization;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests
{
    public class GangHandlerTests
    {
        readonly GangParameters gangParameters = new GangParameters("gangId", null);

        [Fact]
        public async Task first_connection_sent_host_message()
        {
            var handler = GetGangHander();
            var firstGangMember = new FakeGangMember("firstGangMember");

            await handler.HandleAsync(gangParameters, firstGangMember);

            Assert.Equal(GangMessageTypes.Host, firstGangMember.MessagesReceived[0].Type);
        }

        [Fact]
        public async Task second_connection_sent_member_message()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            await handler.HandleAsync(gangParameters, firstGangMember);
            await handler.HandleAsync(gangParameters, secondGangMember);

            Assert.Equal(GangMessageTypes.Member, secondGangMember.MessagesReceived[0].Type);
        }

        [Fact]
        public async Task second_connection_sent_disconnect_message_when_first_closes()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            await handler.HandleAsync(gangParameters, firstGangMember);
            await handler.HandleAsync(gangParameters, secondGangMember);

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

            await handler.HandleAsync(gangParameters, firstGangMember);
            await handler.HandleAsync(gangParameters, secondGangMember);
            await handler.HandleAsync(gangParameters, thirdGangMember);

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

            await handler.HandleAsync(gangParameters, hostMember);
            await handler.HandleAsync(gangParameters, otherMember);


            await otherMember.Controller.SendAsync(new byte[] { 10, 1, 1 });

            Assert.Equal(GangMessageTypes.Command, hostMember.MessagesReceived[2].Type);
            Assert.Equal(266, hostMember.MessagesReceived[2].SequenceNumber.Value);
        }

        [Fact]
        public async Task member_add_remove_events()
        {
            var handler = GetGangHander();
            var events = new List<GangEvent>();
            handler.Events.Subscribe(e => events.Add(e));

            var hostMember = new FakeGangMember("hostMember");

            await handler.HandleAsync(gangParameters, hostMember);

            await hostMember.DisconnectAsync();

            Assert.Equal(3, events.Count);
            Assert.IsType<GangAddedEvent>(events[0]);
            Assert.IsType<GangMemberAddedEvent>(events[1]);
            Assert.IsType<GangMemberRemovedEvent>(events[2]);
        }

        [Fact]
        public async Task add_host_member_on_gang_added_event()
        {
            var handler = GetGangHander();

            var hostMember = new FakeGangMember("host");
            var firstGangMember = new FakeGangMember("firstGangMember");

            using (handler.Events
                .OfType<GangAddedEvent>()
                .Subscribe(async e =>

                 await handler.HandleAsync(gangParameters, hostMember).BlockAsync()
             ))
            {
                await handler.HandleAsync(gangParameters, firstGangMember);

                var gang = handler.GangById(gangParameters.GangId);
                Assert.Equal(2, gang.Members.Count);

                Assert.Equal(hostMember, gang.HostMember);
            }
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
