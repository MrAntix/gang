using System;

namespace Gang.State.Events
{
    [Serializable]
    public sealed class GangStateEventNotFoundException :
        Exception
    {
        public GangStateEventNotFoundException(
            string key) : base($"Event not found {key}")
        {
            Key = key;
        }

        public string Key { get; }
    }
}
