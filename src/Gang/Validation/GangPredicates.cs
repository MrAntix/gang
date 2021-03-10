using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gang.Validation
{
    public static class GangPredicates
    {
        public static bool Is<T>(this T value, Func<T, bool> test)
        {
            return test(value);
        }

        public static bool IsNot<T>(this T value, Func<T, bool> test)
        {
            return !test(value);
        }

        public static bool Null<T>(T value)
        {
            return value == null;
        }

        public static bool NullOrEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool NullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static Func<string, bool> EqualTo(
            string value,
            StringComparison comparison = StringComparison.CurrentCultureIgnoreCase
            )
        {
            return other => string.Compare(value, other, comparison) == 0;
        }

        public static Func<T, bool> EqualTo<T>(
           T value
           ) where T : IComparable
        {
            return other => value.CompareTo(other) == 0;
        }

        public static Func<T, bool> GreaterThan<T>(
            T value
            ) where T : IComparable
        {
            return other => value.CompareTo(other) < 0;
        }

        public static Func<T, bool> GreaterThanOrEqualTo<T>(
           T value
           ) where T : IComparable
        {
            return other => value.CompareTo(other) <= 0;
        }

        public static Func<T, bool> LessThan<T>(
            T value
            ) where T : IComparable
        {
            return other => value.CompareTo(other) > 0;
        }

        public static Func<T, bool> LessThanOrEqualTo<T>(
           T value
           ) where T : IComparable
        {
            return other => value.CompareTo(other) >= 0;
        }

        public static readonly Regex EMAIL_REGEX = new(
            "^[-!#$%&'*+\\/0-9=?A-Z^_a-z{|}~](\\.?[-!#$%&'*+\\/0-9=?A-Z^_a-z`{|}~])*@[a-zA-Z0-9](-*\\.?[a-zA-Z0-9])*\\.[a-zA-Z](-?[a-zA-Z0-9])+$",
            RegexOptions.Compiled);
        public static bool EmailAddress(string value)
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
        }
    }
}
