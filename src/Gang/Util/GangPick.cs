using System;
using System.Linq;

namespace Gang.Util
{
    public static class GangPick
    {
        static readonly Random _rnd = new Random();

        public static T OneOf<T>(
            )
            where T : Enum
        {
            var values = Enum.GetValues(typeof(T))
                .Cast<T>();

            return values.ElementAt(_rnd.Next(values.Count()));
        }

        public static int Number(int from, int to)
        {
            return _rnd.Next(from, to);
        }
    }
}
