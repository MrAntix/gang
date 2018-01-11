using Gang.Contracts;
using Gang.WebSockets.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests
{
    public class GangHandlerTests
    {
        readonly GangParameters gangParameters = new GangParameters("gangId");

        [Fact]
        public async Task first_connection_sent_host_message()
        {
            var handler = GetGangHander();
            var firstGangMember = new FakeGangMember("firstGangMember");

            await handler.HandleAsync(gangParameters, firstGangMember);

            Assert.Equal(GangMessageTypes.Host, firstGangMember.Sent[0].Item1);
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

            Assert.Equal(GangMessageTypes.Member, secondGangMember.Sent[0].Item1);
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

            Assert.Equal(GangMessageTypes.Disconnect, secondGangMember.Sent[1].Item1);
        }

        [Fact]
        public async Task host_send_will_broadcast_state()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");
            var thirdGangMember = new FakeGangMember("thirdGangMember");

            firstGangMember.OnReceiveAction(async () =>
            {
                await Task.Delay(10);
                firstGangMember.IsConnected = false;

                return new[] { (byte)1 };
            });

            await Task.WhenAll(
                handler.HandleAsync(gangParameters, firstGangMember),
                handler.HandleAsync(gangParameters, secondGangMember),
                handler.HandleAsync(gangParameters, thirdGangMember)
            );

            Assert.Equal(GangMessageTypes.State, secondGangMember.Sent[1].Item1);
            Assert.Equal(GangMessageTypes.State, thirdGangMember.Sent[1].Item1);
        }

        [Fact]
        public async Task member_send_will_send_command_to_host()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            secondGangMember.OnReceiveAction(async () =>
            {
                await Task.Delay(10);
                secondGangMember.IsConnected = false;

                return new[] { (byte)1 };
            });

            await Task.WhenAll(
                handler.HandleAsync(gangParameters, firstGangMember),
                handler.HandleAsync(gangParameters, secondGangMember)
            );

            Assert.Equal(GangMessageTypes.Command, firstGangMember.Sent[1].Item1);
        }

        [Fact]
        public void member_can_be_disconnected()
        {
            var handler = GetGangHander();

            var firstGangMember = new FakeGangMember("firstGangMember");

            firstGangMember.OnReceiveAction(async () =>
            {
                await Task.Delay(10);
                var member = handler
                    .GangById(gangParameters.GangId)
                    .MemberById(firstGangMember.Id);

                await member.DisconnectAsync();

                return default(byte[]);
            });

            Assert.True(
                handler
                    .HandleAsync(gangParameters, firstGangMember)
                    .Wait(100)
                );
        }

        IGangHandler GetGangHander(
            GangCollection gangs = null)
        {
            return new GangHandler(
                new JsonSerializationService(),
                gangs = new GangCollection());
        }
    }
}
