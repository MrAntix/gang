namespace Gang.Tests.StatefulHost
{
    public class SetCommand
    {
        public SetCommand(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }
}
