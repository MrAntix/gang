using Gang.Authentication;
using Gang.WebSockets;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests
{
    public sealed class WebSocketGangAuthenticationenticatorTests
    {
        readonly string MEMBER_ID = "MEMBER_ID";
        readonly GangParameters parmeters = new("GANG_ID", "TOKEN");

        [Fact]
        public async Task gets_an_id_and_token()
        {
            var auth = GetService(
                parameter => Task.FromResult(new GangAuth(
                    MEMBER_ID,
                    null,
                    parameter.Token
                    ))
                );
            var member = new FakeGangMember(MEMBER_ID);

            await auth.ExecuteAsync(
                parmeters, GetMember(member));

            Assert.True(member.Connected);
        }

        [Fact]
        public async Task gets_an_id_but_no_token()
        {
            var auth = GetService(
                parameter => Task.FromResult(new GangAuth(
                    MEMBER_ID,
                    null
                    ))
                );
            var member = new FakeGangMember(MEMBER_ID);

            await auth.ExecuteAsync(
                parmeters, GetMember(member));

            Assert.True(member.Connected);
        }

        [Fact]
        public async Task gets_no_id_or_token()
        {
            var auth = GetService(
                parameter => Task.FromResult(new GangAuth(
                    null,
                    null
                    ))
                );
            var member = new FakeGangMember(MEMBER_ID);

            await auth.ExecuteAsync(
                parmeters, GetMember(member));

            Assert.True(member.Disconnected);
            Assert.Equal(
                WebSocketGangAuthenticationenticator.RESULT_DENIED,
                member.DisconnectedReason);
        }

        [Fact]
        public async Task gets_null()
        {
            var auth = GetService(
                parameter => Task.FromResult(default(GangAuth))
                );
            var member = new FakeGangMember(MEMBER_ID);

            await auth.ExecuteAsync(
                parmeters, GetMember(member));

            Assert.True(member.Disconnected);
            Assert.Equal(
                WebSocketGangAuthenticationenticator.RESULT_DENIED,
                member.DisconnectedReason);
        }

        static IWebSocketGangAuthenticationService GetService(
           GangAuthenticationFunc authenticateAsync)
        {
            return new WebSocketGangAuthenticationenticator(
                authenticateAsync,
                new FakeGangManager()
            );
        }

        static Func<GangAuth, Task<IGangMember>> GetMember(
            FakeGangMember member)
        {
            return _ => Task.FromResult((IGangMember)member);
        }
    }
}
