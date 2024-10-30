﻿using System.Text.RegularExpressions;

namespace TestingPlatform.Utilities
{
    public static class StringExtensions
    {
        public static bool RegexIsMatch(this string input, string pattern)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pattern, nameof(pattern));

            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return Regex.IsMatch(input, pattern);
        }
    }
}