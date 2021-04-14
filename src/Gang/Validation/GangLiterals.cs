using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gang.Validation
{
    public static class GangLiterals
    {
        public static readonly Regex EMAIL_REGEX = new(
            "^[-!#$%&'*+\\/0-9=?A-Z^_a-z{|}~](\\.?[-!#$%&'*+\\/0-9=?A-Z^_a-z`{|}~])*@[a-zA-Z0-9](-*\\.?[a-zA-Z0-9])*\\.[a-zA-Z](-?[a-zA-Z0-9])+$",
            RegexOptions.Compiled);

        public static readonly Literal<string> EmailAddress =
            new((string value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;

                if (value.Length > 254) return false;

                var valid = EMAIL_REGEX.IsMatch(value);
                if (!valid) return false;

                var parts = value.Split('@');
                if (parts[0].Length > 64) return false;

                var domainParts = parts[1].Split('.');
                if (domainParts.Any(part => part.Length > 63))
                    return false;

                return true;
            });

        public static readonly Literal<string> NullOrEmpty =
            new((string value) => string.IsNullOrEmpty(value));

        public static readonly Literal<string> NullOrWhiteSpace =
            new((string value) => string.IsNullOrWhiteSpace(value));

        public class Literal<T>
        {
            internal Literal(Func<T, bool> test)
            {
                Test = test;
            }

            public Func<T, bool> Test { get; }

            public static bool operator ==(T value, Literal<T> func)
            {
                return func.Test(value);
            }
            public static bool operator !=(T value, Literal<T> func)
            {

                return !func.Test(value);
            }

            public override bool Equals(object value)
            {
                return Test((T)value);
            }

            public override int GetHashCode()
            {
                return Test.GetHashCode();
            }
        }
    }
}
