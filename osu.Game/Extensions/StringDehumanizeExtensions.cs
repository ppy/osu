// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// Based on code from the Humanizer library (https://github.com/Humanizr/Humanizer/blob/606e958cb83afc9be5b36716ac40d4daa9fa73a7/src/Humanizer/InflectorExtensions.cs)
//
// Humanizer is licenced under the MIT License (MIT)
//
// Copyright (c) .NET Foundation and Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Text.RegularExpressions;

namespace osu.Game.Extensions
{
    /// <summary>
    /// Class with extension methods used to turn human-readable strings to casing conventions frequently used in code.
    /// Often used for communicating with other systems (web API, spectator server).
    /// All of the operations in this class are intentionally culture-invariant.
    /// </summary>
    public static class StringDehumanizeExtensions
    {
        /// <summary>
        /// Converts the string to "Pascal case" (also known as "upper camel case").
        /// </summary>
        /// <example>
        /// <code>
        /// "this is a test string".ToPascalCase() == "ThisIsATestString"
        /// </code>
        /// </example>
        public static string ToPascalCase(this string input)
        {
            return Regex.Replace(input, "(?:^|_|-| +)(.)", match => match.Groups[1].Value.ToUpperInvariant());
        }

        /// <summary>
        /// Converts the string to (lower) "camel case".
        /// </summary>
        /// <example>
        /// <code>
        /// "this is a test string".ToCamelCase() == "thisIsATestString"
        /// </code>
        /// </example>
        public static string ToCamelCase(this string input)
        {
            string word = input.ToPascalCase();
            return word.Length > 0 ? word.Substring(0, 1).ToLowerInvariant() + word.Substring(1) : word;
        }

        /// <summary>
        /// Converts the string to "snake case".
        /// </summary>
        /// <example>
        /// <code>
        /// "this is a test string".ToSnakeCase() == "this_is_a_test_string"
        /// </code>
        /// </example>
        public static string ToSnakeCase(this string input)
        {
            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(input, @"([\p{Lu}]+)([\p{Lu}][\p{Ll}])", "$1_$2"), @"([\p{Ll}\d])([\p{Lu}])", "$1_$2"), @"[-\s]", "_").ToLowerInvariant();
        }

        /// <summary>
        /// Converts the string to "kebab case".
        /// </summary>
        /// <example>
        /// <code>
        /// "this is a test string".ToKebabCase() == "this-is-a-test-string"
        /// </code>
        /// </example>
        public static string ToKebabCase(this string input)
        {
            return ToSnakeCase(input).Replace('_', '-');
        }
    }
}
