using Xunit;

namespace Gang.Tests
{
    public class GangCollectionTests
    {
        [Fact]
        public void first_member_becomes_host()
        {
            var gangs = new GangCollection();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            gangs.AddMember("gangId", firstGangMember);
            var gang = gangs.AddMember("gangId", secondGangMember);

            Assert.True(gang.Host == firstGangMember);
        }

        [Fact]
        public void when_host_leaves_first_member_becomes_host()
        {
            var gangs = new GangCollection();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            gangs.AddMember("gangId", firstGangMember);
            gangs.AddMember("gangId", secondGangMember);

            var gang = gangs.RemoveMember("gangId", firstGangMember);

            Assert.True(gang.Host == secondGangMember);
        }

        [Fact]
        public void gang_is_removed_when_last_member_leaves()
        {
            var gangs = new GangCollection();

            var gangMember = new FakeGangMember("gangMember");
            gangs.AddMember("gangId", gangMember);
            gangs.RemoveMember("gangId", gangMember);

            Assert.Null(gangs["gangId"]);
        }

    }
}
