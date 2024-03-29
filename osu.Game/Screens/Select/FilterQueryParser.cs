﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    /// <summary>
    /// Utility class used for parsing song select filter queries entered via the search box.
    /// </summary>
    public static class FilterQueryParser
    {
        private static readonly Regex query_syntax_regex = new Regex(
            @"\b(?<key>\w+)(?<op>(:|=|(>|<)(:|=)?))(?<value>("".*""[!]?)|(\S*))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static void ApplyQueries(FilterCriteria criteria, string query)
        {
            foreach (Match match in query_syntax_regex.Matches(query))
            {
                string key = match.Groups["key"].Value.ToLowerInvariant();
                var op = parseOperator(match.Groups["op"].Value);
                string value = match.Groups["value"].Value;

                if (tryParseKeywordCriteria(criteria, key, value, op))
                    query = query.Replace(match.ToString(), "");
            }

            criteria.SearchText = query;
        }

        private static bool tryParseKeywordCriteria(FilterCriteria criteria, string key, string value, Operator op)
        {
            switch (key)
            {
                case "star":
                case "stars":
                    return TryUpdateCriteriaRange(ref criteria.StarDifficulty, op, value, 0.01d / 2);

                case "ar":
                    return TryUpdateCriteriaRange(ref criteria.ApproachRate, op, value);

                case "dr":
                case "hp":
                    return TryUpdateCriteriaRange(ref criteria.DrainRate, op, value);

                case "cs":
                    return TryUpdateCriteriaRange(ref criteria.CircleSize, op, value);

                case "od":
                    return TryUpdateCriteriaRange(ref criteria.OverallDifficulty, op, value);

                case "bpm":
                    return TryUpdateCriteriaRange(ref criteria.BPM, op, value, 0.01d / 2);

                case "length":
                    return tryUpdateLengthRange(criteria, op, value);

                case "played":
                case "lastplayed":
                    return tryUpdateDateAgoRange(ref criteria.LastPlayed, op, value);

                case "divisor":
                    return TryUpdateCriteriaRange(ref criteria.BeatDivisor, op, value, tryParseInt);

                case "status":
                    return TryUpdateCriteriaSet(ref criteria.OnlineStatus, op, value);

                case "creator":
                case "author":
                case "mapper":
                    return TryUpdateCriteriaText(ref criteria.Creator, op, value);

                case "artist":
                    return TryUpdateCriteriaText(ref criteria.Artist, op, value);

                case "title":
                    return TryUpdateCriteriaText(ref criteria.Title, op, value);

                case "diff":
                    return TryUpdateCriteriaText(ref criteria.DifficultyName, op, value);

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

        private static bool tryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct
        {
            // First try an exact match.
            if (Enum.TryParse(value, true, out result))
                return true;

            // Then try a prefix match.
            string? prefixMatch = Enum.GetNames(typeof(TEnum)).FirstOrDefault(name => name.StartsWith(value, true, CultureInfo.InvariantCulture));

            if (prefixMatch == null)
                return false;

            return Enum.TryParse(prefixMatch, true, out result);
        }

        private static GroupCollection? tryMatchRegex(string value, string regex)
        {
            Match matches = Regex.Match(value, regex);

            if (matches.Success)
                return matches.Groups;

            return null;
        }

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
                    textFilter.SearchTerm = value;
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

        /// <summary>
        /// Attempts to parse a keyword filter of type <typeparamref name="T"/>,
        /// from the specified <paramref name="op"/> and <paramref name="filterValue"/>.
        /// If <paramref name="filterValue"/> can be parsed successfully, the function returns <c>true</c>
        /// and the resulting range constraint is stored into the <paramref name="range"/>'s expected values.
        /// </summary>
        /// <param name="range">The <see cref="FilterCriteria.OptionalSet{T}"/> to store the parsed data into, if successful.</param>
        /// <param name="op">The operator for the keyword filter.</param>
        /// <param name="filterValue">The value of the keyword filter.</param>
        public static bool TryUpdateCriteriaSet<T>(ref FilterCriteria.OptionalSet<T> range, Operator op, string filterValue)
            where T : struct, Enum
        {
            var matchingValues = new HashSet<T>();

            if (op == Operator.Equal && filterValue.Contains(','))
            {
                string[] splitValues = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (string splitValue in splitValues)
                {
                    if (!tryParseEnum<T>(splitValue, out var parsedValue))
                        return false;

                    matchingValues.Add(parsedValue);
                }
            }
            else
            {
                if (!tryParseEnum<T>(filterValue, out var pivotValue))
                    return false;

                var allDefinedValues = Enum.GetValues<T>();

                foreach (var val in allDefinedValues)
                {
                    int compareResult = Comparer<T>.Default.Compare(val, pivotValue);

                    switch (op)
                    {
                        case Operator.Less:
                            if (compareResult < 0) matchingValues.Add(val);
                            break;

                        case Operator.LessOrEqual:
                            if (compareResult <= 0) matchingValues.Add(val);
                            break;

                        case Operator.Equal:
                            if (compareResult == 0) matchingValues.Add(val);
                            break;

                        case Operator.GreaterOrEqual:
                            if (compareResult >= 0) matchingValues.Add(val);
                            break;

                        case Operator.Greater:
                            if (compareResult > 0) matchingValues.Add(val);
                            break;

                        default:
                            return false;
                    }
                }
            }

            range.Values.IntersectWith(matchingValues);
            return true;
        }

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
            List<string> parts = new List<string>();

            GroupCollection? match = null;

            match ??= tryMatchRegex(val, @"^((?<hours>\d+):)?(?<minutes>\d+):(?<seconds>\d+)$");
            match ??= tryMatchRegex(val, @"^((?<hours>\d+(\.\d+)?)h)?((?<minutes>\d+(\.\d+)?)m)?((?<seconds>\d+(\.\d+)?)s)?$");
            match ??= tryMatchRegex(val, @"^(?<seconds>\d+(\.\d+)?)$");

            if (match == null)
                return false;

            if (match["seconds"].Success)
                parts.Add(match["seconds"].Value + "s");
            if (match["minutes"].Success)
                parts.Add(match["minutes"].Value + "m");
            if (match["hours"].Success)
                parts.Add(match["hours"].Value + "h");

            double totalLength = 0;
            int minScale = 3600000;

            for (int i = 0; i < parts.Count; i++)
            {
                string part = parts[i];
                string partNoUnit = part.TrimEnd('m', 's', 'h');
                if (!tryParseDoubleWithPoint(partNoUnit, out double length))
                    return false;

                if (i != parts.Count - 1 && length >= 60)
                    return false;
                if (i != 0 && partNoUnit.Contains('.'))
                    return false;

                int scale = getLengthScale(part);
                totalLength += length * scale;
                minScale = Math.Min(minScale, scale);
            }

            return tryUpdateCriteriaRange(ref criteria.Length, op, totalLength, minScale / 2.0);
        }

        /// <summary>
        /// This function is intended for parsing "days / months / years ago" type filters.
        /// </summary>
        private static bool tryUpdateDateAgoRange(ref FilterCriteria.OptionalRange<DateTimeOffset> dateRange, Operator op, string val)
        {
            switch (op)
            {
                case Operator.Equal:
                    // an equality filter is difficult to define for support here.
                    // if "3 months 2 days ago" means a single concrete time instant, such a filter is basically useless.
                    // if it means a range of 24 hours, then that is annoying to write and also comes with its own implications
                    // (does it mean "time instant 3 months 2 days ago, within 12 hours of tolerance either direction"?
                    // does it mean "the full calendar day, from midnight to midnight, 3 months 2 days ago"?)
                    // as such, for simplicity, just refuse to support this.
                    return false;

                // for the remaining operators, since the value provided to this function is an "ago" type value
                // (as in, referring to some amount of time back),
                // we'll want to flip the operator, such that `>5d` means "more than five days ago", as in "*before* five days ago",
                // as intended by the user.
                case Operator.Less:
                    op = Operator.Greater;
                    break;

                case Operator.LessOrEqual:
                    op = Operator.GreaterOrEqual;
                    break;

                case Operator.Greater:
                    op = Operator.Less;
                    break;

                case Operator.GreaterOrEqual:
                    op = Operator.LessOrEqual;
                    break;
            }

            GroupCollection? match = null;

            match ??= tryMatchRegex(val, @"^((?<years>\d+)y)?((?<months>\d+)M)?((?<days>\d+(\.\d+)?)d)?((?<hours>\d+(\.\d+)?)h)?((?<minutes>\d+(\.\d+)?)m)?((?<seconds>\d+(\.\d+)?)s)?$");
            match ??= tryMatchRegex(val, @"^(?<days>\d+(\.\d+)?)$");

            if (match == null)
                return false;

            DateTimeOffset? dateTimeOffset = null;
            DateTimeOffset now = DateTimeOffset.Now;

            try
            {
                List<string> keys = new List<string> { @"seconds", @"minutes", @"hours", @"days", @"months", @"years" };

                foreach (string key in keys)
                {
                    if (!match.TryGetValue(key, out var group) || !group.Success)
                        continue;

                    if (group.Success)
                    {
                        if (!tryParseDoubleWithPoint(group.Value, out double length))
                            return false;

                        switch (key)
                        {
                            case @"seconds":
                                dateTimeOffset = (dateTimeOffset ?? now).AddSeconds(-length);
                                break;

                            case @"minutes":
                                dateTimeOffset = (dateTimeOffset ?? now).AddMinutes(-length);
                                break;

                            case @"hours":
                                dateTimeOffset = (dateTimeOffset ?? now).AddHours(-length);
                                break;

                            case @"days":
                                dateTimeOffset = (dateTimeOffset ?? now).AddDays(-length);
                                break;

                            case @"months":
                                dateTimeOffset = (dateTimeOffset ?? now).AddMonths(-(int)length);
                                break;

                            case @"years":
                                dateTimeOffset = (dateTimeOffset ?? now).AddYears(-(int)length);
                                break;
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                dateTimeOffset = DateTimeOffset.MinValue.AddMilliseconds(1);
            }

            if (!dateTimeOffset.HasValue)
                return false;

            return tryUpdateCriteriaRange(ref dateRange, op, dateTimeOffset.Value);
        }
    }
}
