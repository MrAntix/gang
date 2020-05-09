using System;
using Xunit;

namespace Gang.Tests
{
    public class GangExtensionTests
    {
        [Theory]
        [InlineData(typeof(ACommand), "A")]
        [InlineData(typeof(ABC), "ABC")]
        [InlineData(typeof(Other), Other.TYPE)]
        public void GetCommandType(
            Type type,
            string expected)
        {
            var command = Activator.CreateInstance(type);
            Assert.Equal(expected, GangExtensions.GetCommandType(command));
        }

        class ACommand { }
        class ABC { }
        class Other : IGangCommand
        {
            public const string TYPE = "different";
            public string CommandType => TYPE;
        }
    }
}
