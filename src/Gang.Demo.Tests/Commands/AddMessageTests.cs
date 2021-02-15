using Gang.Demo.Web.Server.Commands;
using Gang.Demo.Web.Server.Events;
using Gang.Demo.Web.Server.State;
using Gang.State;
using Gang.State.Commands;
using Gang.Tests.State.Fakes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Demo.Tests.Commands
{
    public sealed class AddMessageTests
    {
        const string GANG_ID = "GANG_ID";
        const string USER_ID = "USER_ID";
        const string MESSAGE_ID = "MESSAGE_ID";
        const string MESSAGE_TEXT = "MESSAGE_TEXT";
        static readonly DateTimeOffset MESSAGE_ON = DateTimeOffset.Now;

        [Fact]
        public async Task success()
        {
            var handler = GetHandler();
            var state = GetState();

            var command = GangCommand.From(
                new AddMessage(MESSAGE_ID, MESSAGE_TEXT),
                new GangAudit(GANG_ID, 1, userId: USER_ID, on: MESSAGE_ON)
            );

            var result = await handler
                .HandleAsync(state, command);

            Assert.Null(result.Errors);

            Assert.IsType<MessageAdded>(
                Assert.Single(result.Uncommitted)
                );

            var message = Assert.Single(result.Data.Messages);
            Assert.Equal(MESSAGE_ID, message.Id);
            Assert.Equal(MESSAGE_TEXT, message.Text);
            Assert.Equal(MESSAGE_ON, message.On);
            Assert.Equal(USER_ID, message.ById);
        }

        [Fact]
        public async Task error_when_user_not_found()
        {
            var handler = GetHandler();
            var state = GetState();

            var command = GangCommand.From(
                new AddMessage(MESSAGE_ID, MESSAGE_TEXT),
                new GangAudit(GANG_ID, 1, userId: "NOT_FOUND")
            );

            var result = await handler
                .HandleAsync(state, command);

            Assert.Equal(User.ERROR_USER_NOT_FOUND, Assert.Single(result.Errors));
        }

        [Fact]
        public async Task error_when_text_is_empty()
        {
            var handler = GetHandler();
            var state = GetState();

            var command = GangCommand.From(
                new AddMessage(MESSAGE_ID, string.Empty),
                new GangAudit(GANG_ID, 1, userId: USER_ID)
            );

            var result = await handler
                .HandleAsync(state, command);

            Assert.Equal(
                Message.ERROR_MESSAGE_TEXT_IS_REQUIRED,
                Assert.Single(result.Errors)
                );
        }

        [Fact]
        public async Task empty_id_random_assigned()
        {
            var handler = GetHandler();
            var state = GetState();

            var command = GangCommand.From(
                new AddMessage(string.Empty, MESSAGE_TEXT),
                new GangAudit(GANG_ID, 1, userId: USER_ID)
            );

            var result = await handler
                .HandleAsync(state, command);

            Assert.Null(result.Errors);

            Assert.NotNull(
                Assert.Single(result.Data.Messages).Id
                );
        }

        static IGangCommandHandler<HostState, AddMessage> GetHandler()
        {
            return new AddMessageHandler();
        }

        static GangState<HostState> GetState()
        {
            var state = new HostState(
               users: new[] { new User(USER_ID) }
               );

            return GangState.Create(state);
        }
    }
}
