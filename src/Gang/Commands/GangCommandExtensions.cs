using System;

namespace Gang.Commands
{
    public static class GangCommandExtensions
    {
        public static string GetCommandTypeName(
            this Type type)
        {
            return type.Name
                .TryDecapitalize()
                .TryTrimEnd("Command");
        }
    }
}