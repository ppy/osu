// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    internal static class FilterQueryParser
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
                case "stars" when parseFloatWithPoint(value, out var stars):
                    return updateCriteriaRange(ref criteria.StarDifficulty, op, stars, 0.01f / 2);

                case "ar" when parseFloatWithPoint(value, out var ar):
                    return updateCriteriaRange(ref criteria.ApproachRate, op, ar, 0.1f / 2);

                case "dr" when parseFloatWithPoint(value, out var dr):
                case "hp" when parseFloatWithPoint(value, out dr):
                    return updateCriteriaRange(ref criteria.DrainRate, op, dr, 0.1f / 2);

                case "cs" when parseFloatWithPoint(value, out var cs):
                    return updateCriteriaRange(ref criteria.CircleSize, op, cs, 0.1f / 2);

                case "bpm" when parseDoubleWithPoint(value, out var bpm):
                    return updateCriteriaRange(ref criteria.BPM, op, bpm, 0.01d / 2);

                case "length" when parseDoubleWithPoint(value.TrimEnd('m', 's', 'h'), out var length):
                    var scale = getLengthScale(value);
                    return updateCriteriaRange(ref criteria.Length, op, length * scale, scale / 2.0);

                case "divisor" when parseInt(value, out var divisor):
                    return updateCriteriaRange(ref criteria.BeatDivisor, op, divisor);

                case "status" when Enum.TryParse<BeatmapSetOnlineStatus>(value, true, out var statusValue):
                    return updateCriteriaRange(ref criteria.OnlineStatus, op, statusValue);

                case "creator":
                    return updateCriteriaText(ref criteria.Creator, op, value);

                case "artist":
                    return updateCriteriaText(ref criteria.Artist, op, value);

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

        private static bool parseFloatWithPoint(string value, out float result) =>
            float.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);

        private static bool parseDoubleWithPoint(string value, out double result) =>
            double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);

        private static bool parseInt(string value, out int result) =>
            int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out result);

        private static bool updateCriteriaText(ref FilterCriteria.OptionalTextFilter textFilter, Operator op, string value)
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

        private static bool updateCriteriaRange(ref FilterCriteria.OptionalRange<float> range, Operator op, float value, float tolerance = 0.05f)
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

        private static bool updateCriteriaRange(ref FilterCriteria.OptionalRange<double> range, Operator op, double value, double tolerance = 0.05)
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

        private static bool updateCriteriaRange<T>(ref FilterCriteria.OptionalRange<T> range, Operator op, T value)
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
    }
}
