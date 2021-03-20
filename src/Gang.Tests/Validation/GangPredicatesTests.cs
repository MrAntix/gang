using Gang.Validation;
using Xunit;
using static Gang.Validation.GangPredicates;

namespace Gang.Tests.Validation
{
    public class GangPredicatesTests
    {
        [Theory]
        [InlineData(1, 0, false)]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, false)]
        public void equal_to(int a, int b, bool expected)
        {

            Assert.Equal(expected, a.Is(EqualTo(b)));
        }

        [Theory]
        [InlineData(1, 0, true)]
        [InlineData(1, 1, false)]
        [InlineData(1, 2, false)]
        public void greater_than(int a, int b, bool expected)
        {

            Assert.Equal(expected, a.Is(GreaterThan(b)));
        }

        [Theory]
        [InlineData(1, 0, true)]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, false)]
        public void greater_than_equal_to(int a, int b, bool expected)
        {

            Assert.Equal(expected, a.Is(GreaterThanOrEqualTo(b)));
        }

        [Theory]
        [InlineData(1, 0, false)]
        [InlineData(1, 1, false)]
        [InlineData(1, 2, true)]
        public void less_than(int a, int b, bool expected)
        {

            Assert.Equal(expected, a.Is(LessThan(b)));
        }

        [Theory]
        [InlineData(1, 0, false)]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, true)]
        public void less_than_equal_to(int a, int b, bool expected)
        {

            Assert.Equal(expected, a.Is(LessThanOrEqualTo(b)));
        }
    }
}