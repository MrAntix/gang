using Gang.Contracts;
using Gang.Events;
using System;
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

            Assert.Equal(GangMessageTypes.Host, firstGangMember.Received[0].Item1);
        }

        [Fact]
        public async Task second_connection_sent_member_message()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            await Task.WhenAll(
                handler.HandleAsync(gangParameters, firstGangMember),
                handler.HandleAsync(gangParameters, secondGangMember)
            );

            Assert.Equal(GangMessageTypes.Member, secondGangMember.Received[0].Item1);
        }

        [Fact]
        public async Task second_connection_sent_disconnect_message_when_first_closes()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember", 10);
            var secondGangMember = new FakeGangMember("secondGangMember", 100);

            await Task.WhenAll(
                handler.HandleAsync(gangParameters, firstGangMember),
                handler.HandleAsync(gangParameters, secondGangMember)
            );

            Assert.Equal(GangMessageTypes.Disconnect, secondGangMember.Received[1].Item1);
        }

        [Fact]
        public async Task host_send_will_broadcast_state()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");
            var thirdGangMember = new FakeGangMember("thirdGangMember");

            firstGangMember.OnConnect = async (onReceiveAsync) =>
            {
                await Task.Delay(10);
                await onReceiveAsync(new[] { (byte)1 });
            };

            await Task.WhenAll(
                handler.HandleAsync(gangParameters, firstGangMember),
                handler.HandleAsync(gangParameters, secondGangMember),
                handler.HandleAsync(gangParameters, thirdGangMember)
            );

            Assert.Equal(GangMessageTypes.State, secondGangMember.Received[1].Item1);
            Assert.Equal(GangMessageTypes.State, thirdGangMember.Received[1].Item1);
        }

        [Fact]
        public async Task member_send_will_send_command_to_host()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            secondGangMember.OnConnect = async (onReceiveAsync) =>
            {
                await Task.Delay(10);
                await onReceiveAsync(new[] { (byte)1 });
            };

            await Task.WhenAll(
                handler.HandleAsync(gangParameters, firstGangMember),
                handler.HandleAsync(gangParameters, secondGangMember)
            );

            Assert.Equal(GangMessageTypes.Command, firstGangMember.Received[2].Item1);
        }

        [Fact]
        public void member_can_be_disconnected()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");

            firstGangMember.OnConnect = async (onReceiveAsync) =>
             {
                 await Task.Delay(10);
                 var member = handler
                     .GangById(gangParameters.GangId)
                     .MemberById(firstGangMember.Id);

                 await member.DisconnectAsync();
             };

            Assert.True(
                handler
                    .HandleAsync(gangParameters, firstGangMember)
                    .Wait(100)
                );
        }

        [Fact]
        public void add_host_member_on_gang_added_event()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var hostMember = new FakeGangMember("host");

            using (handler.Events
                .OfType<GangAddedEvent>()
                .Subscribe(async e =>

                 await handler.HandleAsync(new GangParameters(e.GangId), hostMember)
             ))
            {
                handler.HandleAsync(gangParameters, firstGangMember);

                var gang = handler.GangById(gangParameters.GangId);
                Assert.Equal(2, gang.Members.Count);

                Assert.Equal(hostMember, gang.HostMember);
            }
        }

        IGangHandler GetGangHander(
            GangCollection gangs = null)
        {
            return new GangHandler(
                gangs ?? new GangCollection());
        }
    }
}
