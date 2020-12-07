namespace Gang.Tests
{
    public sealed class TestGangSettings : IGangSettings
    {
        public GangApplication Application
            => new("Tests", "Tests");

        public static readonly IGangSettings Default = new TestGangSettings();
    }
}
