using Xunit;
using static Gang.Validation.GangLiterals;

namespace Gang.Tests.Validation
{
    public class GangLiteralsTests
    {
        [Theory]
        [InlineData("email@example.com")]
        public void email_address(string candidate)
        {

            Assert.True(candidate == EmailAddress);
        }

        [Theory]
        [InlineData("NOT")]
        public void not_email_address(string candidate)
        {

            Assert.True(candidate != EmailAddress);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void null_or_empty(string candidate)
        {

            Assert.True(candidate == NullOrEmpty);
        }

        [Theory]
        [InlineData("NOT")]
        public void not_null_or_empty(string candidate)
        {

            Assert.True(candidate != NullOrEmpty);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void null_or_whitespace(string candidate)
        {

            Assert.True(candidate == NullOrWhiteSpace);
        }

        [Theory]
        [InlineData("NOT")]
        public void not_null_or_whitespace(string candidate)
        {

            Assert.True(candidate != NullOrWhiteSpace);
        }
    }
}
