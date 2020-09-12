using Gang.Commands;
using Gang.Serialization;
using Gang.WebSockets.Serialization;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gang.Tests
{
    public class GangCommandExecutorTests
    {
        [Fact]
        public async Task throws_on_serialization_failure()
        {
            var executor = GetExecutor()
                .Register<Command>(Command.Type, (c) => Task.CompletedTask);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
               executor.ExecuteAsync(_command_null_arg, _audit)
            );
        }

        [Fact]
        public async Task handles_on_serialization_failure()
        {
            var handled = false;

            var executor = GetExecutor()
                .Register<Command>(Command.Type, (c) => Task.CompletedTask)
                .RegisterErrorHandler((d, a, ex) =>
                {
                    handled = true;
                    return Task.CompletedTask;
                });

            await executor.ExecuteAsync(_command_null_arg, _audit);

            Assert.True(handled);
        }

        class Command
        {
            public const string Type = "command";
            public Command(string arg)
            {
                Arg = arg ?? throw new ArgumentNullException(nameof(arg));
            }

            public string Arg { get; }
        }

        static readonly GangMessageAudit _audit = new GangMessageAudit(
            "MemberId".GangToBytes()
            );

        static readonly IGangSerializationService _serializer
            = new WebSocketGangJsonSerializationService();
        static readonly byte[] _command = _serializer.Serialize(new
        {
            arg = "ARG"
        });
        static readonly byte[] _command_null_arg = _serializer.Serialize(new
        {
            arg = default(string)
        });

        IGangCommandExecutor GetExecutor()
        {
            return new GangCommandExecutor(
                _serializer
                );
        }
    }
}
