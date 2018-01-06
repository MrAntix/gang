using Gang.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using Xunit;

namespace Gang.Tests
{
    public class GangConfigurationTests
    {
        [Fact]
        public void when_gang_id_is_null()
        {
            IQueryCollection qs = new QueryCollection();

            var actual = GangConfiguration.GetGangParameters(qs);

            Assert.Null(actual);
        }

        [Fact]
        public void when_gang_id_is_not_null()
        {
            var gangId = "2D71D59D29BB4735B76CCEEAFC8CD652";
            IQueryCollection qs = new QueryCollection(
                new Dictionary<string, StringValues>{
                    {"gangId",gangId}
                });

            var actual = GangConfiguration.GetGangParameters(qs);

            Assert.NotNull(actual);
            Assert.Equal(gangId, actual.GangId);
        }
    }
}
