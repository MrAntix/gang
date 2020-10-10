using Gang.Commands;
using System;
using Xunit;

namespace Gang.Tests
{
    public class GangExtensionTests
    {
        [Theory]
        [InlineData(typeof(ACommand), "a")]
        [InlineData(typeof(ABC), "aBC")]
        public void GetCommandType(
            Type type,
            string expected)
        {
            Assert.Equal(expected, type.GetCommandTypeName());
        }

        class ACommand { }
        class ABC { }
    }
}
