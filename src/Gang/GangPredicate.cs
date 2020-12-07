using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gang
{
    public sealed class GangPredicate
    {
        public static readonly Regex EMAIL_REGEX = new(
            "^[-!#$%&'*+\\/0-9=?A-Z^_a-z{|}~](\\.?[-!#$%&'*+\\/0-9=?A-Z^_a-z`{|}~])*@[a-zA-Z0-9](-*\\.?[a-zA-Z0-9])*\\.[a-zA-Z](-?[a-zA-Z0-9])+$",
            RegexOptions.Compiled);

        public static readonly GangPredicate Is = new(true);
        public static readonly GangPredicate IsNot = new(false);

        readonly bool _success;
        readonly bool _failure;

        public GangPredicate(
            bool truth = false)
        {
            _success = truth;
            _failure = !truth;
        }

        public static string Assert(
            string value, params Func<string, bool>[] assertions)
        {
            foreach (var assertion in assertions)
                if (!assertion(value))
                    throw new Exception($"Failed {assertion.Method.Name}");

            return value;
        }

        public bool EmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return _success;

            if (value.Length > 254) return _failure;

            var valid = EMAIL_REGEX.IsMatch(value);
            if (!valid) return _failure;

            var parts = value.Split('@');
            if (parts[0].Length > 64) return _failure;

            var domainParts = parts[1].Split('.');
            if (domainParts.Any(part => part.Length > 63))
                return _failure;

            return _success;
        }

        public bool Null(string value)
        {
            return value == null ? _success : _failure;
        }

        public bool NullOrEmpty(string value)
        {
            return string.IsNullOrEmpty(value) ? _success : _failure;
        }

        public bool NullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? _success : _failure;
        }
    }
}
