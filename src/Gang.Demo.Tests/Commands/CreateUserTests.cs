using Gang.Demo.Web.Server.Commands;
using Gang.Demo.Web.Server.Events;
using Gang.Demo.Web.Server.State;
using Gang.State;
using Gang.State.Commands;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Demo.Tests.Commands
{
    public sealed class CreateUserTests
    {
        const string GANG_ID = "GANG_ID";
        const string USER_ID = "USER_ID";
        const string USER_NAME = "USER_NAME";

        [Fact]
        public async Task success_create()
        {
            var handler = GetHandler();
            var state = GetState();

            var command = GangCommand.From(
                new UpdateUserName(USER_NAME),
                new GangAudit(GANG_ID, 1, userId: USER_ID)
            );

            var result = await handler
                .HandleAsync(state, command);

            Assert.Null(result.Errors);

            Assert.IsType<UserCreated>(
                result.Uncommitted.ElementAt(0)
                );

            var user = Assert.Single(result.Data.Users);
            Assert.Equal(USER_ID, user.UserId);
            Assert.Equal(USER_NAME, user.Name);

            Assert.IsType<UserMessageAdded>(
                result.Uncommitted.ElementAt(1)
                );
        }

        [Fact]
        public async Task success_update()
        {
            var handler = GetHandler();
            var state = GetState(new User(USER_ID, "OLD_NAME"));

            var command = GangCommand.From(
                new UpdateUserName(USER_NAME),
                new GangAudit(GANG_ID, 1, userId: USER_ID)
            );

            var result = await handler
                .HandleAsync(state, command);

            Assert.Null(result.Errors);

            Assert.IsType<UserNameUpdated>(
                Assert.Single(result.Uncommitted)
                );

            var user = Assert.Single(result.Data.Users);
            Assert.Equal(USER_ID, user.UserId);
            Assert.Equal(USER_NAME, user.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task error_when_name_is_invalid(
            string name)
        {
            var handler = GetHandler();
            var state = GetState(new User(USER_ID, "OLD_NAME"));

            var command = GangCommand.From(
                new UpdateUserName(name),
                new GangAudit(GANG_ID, 1, userId: USER_ID)
            );

            var result = await handler
                .HandleAsync(state, command);

            Assert.Equal(
                User.ERROR_NAME_IS_NOT_VALID,
                Assert.Single(result.Errors)
                );
        }

        [Fact]
        public async Task error_when_name_is_taken()
        {
            var handler = GetHandler();
            var state = GetState(
                new User("OTHER_USER_ID", USER_NAME),
                new User(USER_ID, "OLD_NAME")
                );

            var command = GangCommand.From(
                new UpdateUserName(USER_NAME),
                new GangAudit(GANG_ID, 1, userId: USER_ID)
            );

            var result = await handler
                .HandleAsync(state, command);

            Assert.Equal(
                User.ERROR_NAME_IS_TAKEN,
                Assert.Single(result.Errors)
                );
        }

        static IGangCommandHandler<HostState, UpdateUserName> GetHandler()
        {
            return new UpdateUserNameHandler();
        }

        static GangState<HostState> GetState(
            params User[] users)
        {
            var state = new HostState(
               users: users
               );

            return GangState.Create(state);
        }
    }
}
