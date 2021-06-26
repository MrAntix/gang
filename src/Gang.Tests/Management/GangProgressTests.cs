using Gang.Management;
using Gang.Management.Events;
using System;
using Xunit;

namespace Gang.Tests.Management
{
    public sealed class GangProgressTests
    {
        [Theory]
        [InlineData(2, 10, 40)]
        [InlineData(5, 10, 10)]
        [InlineData(9, 9, 1)]
        public void end_estimate(
            int index, int secondsTaken,
            int expectedSeconds
            )
        {
            var progress = new GangProgressState(
                "TEST", 10, index,
                DateTimeOffset.Now.AddSeconds(-secondsTaken), null
                );
            var actualSeconds = Math.Round(progress.GetEndEstimate().TotalSeconds);

            Assert.Equal(expectedSeconds, actualSeconds);
        }
    }
}
