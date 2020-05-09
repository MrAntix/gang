using System;
using Xunit;

namespace Gang.Tests
{
    public class GangExtensionTests
    {
        [Theory]
        [InlineData(typeof(ACommand), "A")]
        [InlineData(typeof(ABC), "ABC")]
        public void GetCommandType(
            Type type,
            string expected)
        {
            Assert.Equal(expected, type.GetCommandType());
        }

        class ACommand { }
        class ABC { }
    }
}
