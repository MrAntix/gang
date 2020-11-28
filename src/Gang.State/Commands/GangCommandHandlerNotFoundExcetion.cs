using System;

namespace Gang.State.Commands
{
    [Serializable]
    public sealed class GangCommandHandlerNotFoundExcetion : Exception
    {
        public GangCommandHandlerNotFoundExcetion()
        {
        }
    }
}