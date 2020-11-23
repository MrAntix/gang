using Gang.Management;
using Xunit;

namespace Gang.Tests
{
    public class GangCollectionTests
    {
        const string GANG_ID = "GANG_ID";

        [Fact]
        public void first_member_becomes_host()
        {
            var gangs = new GangCollection();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            gangs.AddMemberToGang(GANG_ID, firstGangMember);
            gangs.AddMemberToGang(GANG_ID, secondGangMember);

            Assert.Single(gangs);

            var gang = gangs[GANG_ID];

            Assert.True(gang.HostMember == firstGangMember);
            Assert.True(gang.HostMember != secondGangMember);
        }

        [Fact]
        public void when_host_leaves_first_member_becomes_host()
        {
            var gangs = new GangCollection();

            var firstGangMember = new FakeGangMember("firstGangMember");
            var secondGangMember = new FakeGangMember("secondGangMember");

            gangs.AddMemberToGang(GANG_ID, firstGangMember);
            gangs.AddMemberToGang(GANG_ID, secondGangMember);

            gangs.RemoveMemberFromGang(GANG_ID, firstGangMember);

            var gang = gangs[GANG_ID];

            Assert.True(gang.HostMember == secondGangMember);
        }

        [Fact]
        public void gang_is_removed_when_last_member_leaves()
        {
            var gangs = new GangCollection();

            var gangMember = new FakeGangMember("gangMember");
            gangs.AddMemberToGang(GANG_ID, gangMember);
            gangs.RemoveMemberFromGang(GANG_ID, gangMember);

            Assert.Null(gangs[GANG_ID]);
        }
    }
}
