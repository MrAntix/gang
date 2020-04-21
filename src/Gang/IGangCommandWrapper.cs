namespace Gang
{
    public interface IGangCommandWrapper
    {
        string Type { get; }
        object Command { get; }
    }
}