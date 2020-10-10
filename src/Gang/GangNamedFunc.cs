namespace Gang
{
    public sealed class GangNamedFunc<TFunc>
    {
        public GangNamedFunc(
            string name, TFunc func)
        {
            Name = name;
            Func = func;
        }

        public string Name { get; }
        public TFunc Func { get; }
    }
}
