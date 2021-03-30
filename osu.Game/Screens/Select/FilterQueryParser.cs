﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// Utility class used for parsing song select filter queries entered via the search box.
    /// </summary>
    public static class FilterQueryParser
    {
        private static readonly Regex query_syntax_regex = new Regex(
            @"\b(?<key>\w+)(?<op>(:|=|(>|<)(:|=)?))(?<value>("".*"")|(\S*))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static void ApplyQueries(FilterCriteria criteria, string query)
        {
            foreach (Match match in query_syntax_regex.Matches(query))
            {
                var key = match.Groups["key"].Value.ToLower();
                var op = parseOperator(match.Groups["op"].Value);
                var value = match.Groups["value"].Value;

                if (tryParseKeywordCriteria(criteria, key, value, op))
                    query = query.Replace(match.ToString(), "");
            }

            criteria.SearchText = query;
        }

        private static bool tryParseKeywordCriteria(FilterCriteria criteria, string key, string value, Operator op)
        {
            switch (key)
            {
                case "stars":
                    return TryUpdateCriteriaRange(ref criteria.StarDifficulty, op, value, 0.01d / 2);

                case "ar":
                    return TryUpdateCriteriaRange(ref criteria.ApproachRate, op, value);

                case "dr":
                case "hp":
                    return TryUpdateCriteriaRange(ref criteria.DrainRate, op, value);

                case "cs":
                    return TryUpdateCriteriaRange(ref criteria.CircleSize, op, value);

                case "bpm":
                    return TryUpdateCriteriaRange(ref criteria.BPM, op, value, 0.01d / 2);

                case "length":
                    return tryUpdateLengthRange(criteria, op, value);

                case "divisor":
                    return TryUpdateCriteriaRange(ref criteria.BeatDivisor, op, value, tryParseInt);

                case "status":
                    return TryUpdateCriteriaRange(ref criteria.OnlineStatus, op, value,
                        (string s, out BeatmapSetOnlineStatus val) => Enum.TryParse(value, true, out val));

                case "creator":
                    return TryUpdateCriteriaText(ref criteria.Creator, op, value);

                case "artist":
                    return TryUpdateCriteriaText(ref criteria.Artist, op, value);

                default:
                    return criteria.RulesetCriteria?.TryParseCustomKeywordCriteria(key, op, value) ?? false;
            }
        }

        private static Operator parseOperator(string value)
        {
            switch (value)
            {
                case "=":
                case ":":
                    return Operator.Equal;

                case "<":
                    return Operator.Less;

                case "<=":
                case "<:":
                    return Operator.LessOrEqual;

                case ">":
                    return Operator.Greater;

                case ">=":
                case ">:":
                    return Operator.GreaterOrEqual;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), $"Unsupported operator {value}");
            }
        }

        private static int getLengthScale(string value) =>
            value.EndsWith("ms", StringComparison.Ordinal) ? 1 :
            value.EndsWith('s') ? 1000 :
            value.EndsWith('m') ? 60000 :
            value.EndsWith('h') ? 3600000 : 1000;

        private static bool tryParseFloatWithPoint(string value, out float result) =>
            float.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);

        private static bool tryParseDoubleWithPoint(string value, out double result) =>
            double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);

        private static bool tryParseInt(string value, out int result) =>
            int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out result);

        /// <summary>
        /// Attempts to parse a keyword filter with the specified <paramref name="op"/> and textual <paramref name="value"/>.
        /// If the value indicates a valid textual filter, the function returns <c>true</c> and the resulting data is stored into
        /// <paramref name="textFilter"/>.
        /// </summary>
        /// <param name="textFilter">The <see cref="FilterCriteria.OptionalTextFilter"/> to store the parsed data into, if successful.</param>
        /// <param name="op">
        /// The operator for the keyword filter.
        /// Only <see cref="Operator.Equal"/> is valid for textual filters.
        /// </param>
        /// <param name="value">The value of the keyword filter.</param>
        public static bool TryUpdateCriteriaText(ref FilterCriteria.OptionalTextFilter textFilter, Operator op, string value)
        {
            switch (op)
            {
                case Operator.Equal:
                    textFilter.SearchTerm = value.Trim('"');
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Attempts to parse a keyword filter of type <see cref="float"/>
        /// from the specified <paramref name="op"/> and <paramref name="val"/>.
        /// If <paramref name="val"/> can be parsed as a <see cref="float"/>, the function returns <c>true</c>
        /// and the resulting range constraint is stored into <paramref name="range"/>.
        /// </summary>
        /// <param name="range">
        /// The <see cref="float"/>-typed <see cref="FilterCriteria.OptionalRange{T}"/>
        /// to store the parsed data into, if successful.
        /// </param>
        /// <param name="op">The operator for the keyword filter.</param>
        /// <param name="val">The value of the keyword filter.</param>
        /// <param name="tolerance">Allowed tolerance of the parsed range boundary value.</param>
        public static bool TryUpdateCriteriaRange(ref FilterCriteria.OptionalRange<float> range, Operator op, string val, float tolerance = 0.05f)
            => tryParseFloatWithPoint(val, out float value) && tryUpdateCriteriaRange(ref range, op, value, tolerance);

        private static bool tryUpdateCriteriaRange(ref FilterCriteria.OptionalRange<float> range, Operator op, float value, float tolerance = 0.05f)
        {
            switch (op)
            {
                default:
                    return false;

                case Operator.Equal:
                    range.Min = value - tolerance;
                    range.Max = value + tolerance;
                    break;

                case Operator.Greater:
                    range.Min = value + tolerance;
                    break;

                case Operator.GreaterOrEqual:
                    range.Min = value - tolerance;
                    break;

                case Operator.Less:
                    range.Max = value - tolerance;
                    break;

                case Operator.LessOrEqual:
                    range.Max = value + tolerance;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse a keyword filter of type <see cref="double"/>
        /// from the specified <paramref name="op"/> and <paramref name="val"/>.
        /// If <paramref name="val"/> can be parsed as a <see cref="double"/>, the function returns <c>true</c>
        /// and the resulting range constraint is stored into <paramref name="range"/>.
        /// </summary>
        /// <param name="range">
        /// The <see cref="double"/>-typed <see cref="FilterCriteria.OptionalRange{T}"/>
        /// to store the parsed data into, if successful.
        /// </param>
        /// <param name="op">The operator for the keyword filter.</param>
        /// <param name="val">The value of the keyword filter.</param>
        /// <param name="tolerance">Allowed tolerance of the parsed range boundary value.</param>
        public static bool TryUpdateCriteriaRange(ref FilterCriteria.OptionalRange<double> range, Operator op, string val, double tolerance = 0.05)
            => tryParseDoubleWithPoint(val, out double value) && tryUpdateCriteriaRange(ref range, op, value, tolerance);

        private static bool tryUpdateCriteriaRange(ref FilterCriteria.OptionalRange<double> range, Operator op, double value, double tolerance = 0.05)
        {
            switch (op)
            {
                default:
                    return false;

                case Operator.Equal:
                    range.Min = value - tolerance;
                    range.Max = value + tolerance;
                    break;

                case Operator.Greater:
                    range.Min = value + tolerance;
                    break;

                case Operator.GreaterOrEqual:
                    range.Min = value - tolerance;
                    break;

                case Operator.Less:
                    range.Max = value - tolerance;
                    break;

                case Operator.LessOrEqual:
                    range.Max = value + tolerance;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Used to determine whether the string value <paramref name="val"/> can be converted to type <typeparamref name="T"/>.
        /// If conversion can be performed, the delegate returns <c>true</c>
        /// and the conversion result is returned in the <c>out</c> parameter <paramref name="parsed"/>.
        /// </summary>
        /// <param name="val">The string value to attempt parsing for.</param>
        /// <param name="parsed">The parsed value, if conversion is possible.</param>
        public delegate bool TryParseFunction<T>(string val, out T parsed);

        /// <summary>
        /// Attempts to parse a keyword filter of type <typeparamref name="T"/>,
        /// from the specified <paramref name="op"/> and <paramref name="val"/>.
        /// If <paramref name="val"/> can be parsed into <typeparamref name="T"/> using <paramref name="parseFunction"/>, the function returns <c>true</c>
        /// and the resulting range constraint is stored into <paramref name="range"/>.
        /// </summary>
        /// <param name="range">The <see cref="FilterCriteria.OptionalRange{T}"/> to store the parsed data into, if successful.</param>
        /// <param name="op">The operator for the keyword filter.</param>
        /// <param name="val">The value of the keyword filter.</param>
        /// <param name="parseFunction">Function used to determine if <paramref name="val"/> can be converted to type <typeparamref name="T"/>.</param>
        public static bool TryUpdateCriteriaRange<T>(ref FilterCriteria.OptionalRange<T> range, Operator op, string val, TryParseFunction<T> parseFunction)
            where T : struct
            => parseFunction.Invoke(val, out var converted) && tryUpdateCriteriaRange(ref range, op, converted);

        private static bool tryUpdateCriteriaRange<T>(ref FilterCriteria.OptionalRange<T> range, Operator op, T value)
            where T : struct
        {
            switch (op)
            {
                default:
                    return false;

                case Operator.Equal:
                    range.IsLowerInclusive = range.IsUpperInclusive = true;
                    range.Min = value;
                    range.Max = value;
                    break;

                case Operator.Greater:
                    range.IsLowerInclusive = false;
                    range.Min = value;
                    break;

                case Operator.GreaterOrEqual:
                    range.IsLowerInclusive = true;
                    range.Min = value;
                    break;

                case Operator.Less:
                    range.IsUpperInclusive = false;
                    range.Max = value;
                    break;

                case Operator.LessOrEqual:
                    range.IsUpperInclusive = true;
                    range.Max = value;
                    break;
            }

            return true;
        }

        private static bool tryUpdateLengthRange(FilterCriteria criteria, Operator op, string val)
        {
            if (!tryParseDoubleWithPoint(val.TrimEnd('m', 's', 'h'), out var length))
                return false;

            var scale = getLengthScale(val);
            return tryUpdateCriteriaRange(ref criteria.Length, op, length * scale, scale / 2.0);
        }
    }
}
