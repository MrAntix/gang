namespace Gang
{
    public sealed class GangApplication
    {
        public GangApplication(
            string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
    }

}
