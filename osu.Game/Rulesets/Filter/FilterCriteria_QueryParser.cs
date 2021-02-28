// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Filter
{
    public partial class FilterCriteria
    {
        private static readonly Regex query_syntax_regex = new Regex(
            @"\b(?<key>\w+)(?<op>(:|=|(>|<)(:|=)?))(?<value>("".*"")|(\S*))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private void applyQueries(string query)
        {
            foreach (Match match in query_syntax_regex.Matches(query))
            {
                var key = match.Groups["key"].Value.ToLower();
                var op = parseOperator(match.Groups["op"].Value);
                var value = match.Groups["value"].Value;

                if (tryParseKeywordCriteria(key, value, op))
                    query = query.Replace(match.ToString(), "");
            }

            SearchText = query;
        }

        private bool tryParseKeywordCriteria(string key, string value, Operator op)
        {
            switch (key)
            {
                case "stars" when parseFloatWithPoint(value, out var stars):
                    updateCriteriaRange(ref StarDifficulty, op, stars, 0.01f / 2);
                    break;

                case "ar" when parseFloatWithPoint(value, out var ar):
                    updateCriteriaRange(ref ApproachRate, op, ar, 0.1f / 2);
                    break;

                case "dr" when parseFloatWithPoint(value, out var dr):
                case "hp" when parseFloatWithPoint(value, out dr):
                    updateCriteriaRange(ref DrainRate, op, dr, 0.1f / 2);
                    break;

                case "cs" when parseFloatWithPoint(value, out var cs):
                    updateCriteriaRange(ref CircleSize, op, cs, 0.1f / 2);
                    break;

                case "bpm" when parseDoubleWithPoint(value, out var bpm):
                    updateCriteriaRange(ref BPM, op, bpm, 0.01d / 2);
                    break;

                case "length" when parseDoubleWithPoint(value.TrimEnd('m', 's', 'h'), out var length):
                    var scale = getLengthScale(value);
                    updateCriteriaRange(ref Length, op, length * scale, scale / 2.0);
                    break;

                case "divisor" when parseInt(value, out var divisor):
                    updateCriteriaRange(ref BeatDivisor, op, divisor);
                    break;

                case "status" when Enum.TryParse<BeatmapSetOnlineStatus>(value, true, out var statusValue):
                    updateCriteriaRange(ref OnlineStatus, op, statusValue);
                    break;

                case "creator":
                    updateCriteriaText(ref Creator, op, value);
                    break;

                case "artist":
                    updateCriteriaText(ref Artist, op, value);
                    break;

                default:
                    return TryParseCustomKeywordCriteria(key, value, op);
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse a single custom keyword criterion, given by the user via the song select search box.
        /// The format of the criterion is:
        /// <code>
        /// {key}{op}{value}
        /// </code>
        /// </summary>
        /// <param name="key">The key (name) of the criterion.</param>
        /// <param name="value">The value of the criterion.</param>
        /// <param name="op">The operator in the criterion.</param>
        /// <returns>
        /// <c>true</c> if the keyword criterion is valid, <c>false</c> if it has been ignored.
        /// Valid criteria are stripped from <see cref="SearchText"/>,
        /// while ignored criteria are included in <see cref="SearchText"/>.
        /// </returns>
        protected virtual bool TryParseCustomKeywordCriteria(string key, string value, Operator op) => false;

        private Operator parseOperator(string value)
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

        private static void updateCriteriaText(ref OptionalTextFilter textFilter, Operator op, string value)
        {
            switch (op)
            {
                case Operator.Equal:
                    textFilter.SearchTerm = value.Trim('"');
                    break;
            }
        }

        private static void updateCriteriaRange(ref OptionalRange<float> range, Operator op, float value, float tolerance = 0.05f)
        {
            switch (op)
            {
                default:
                    return;

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
        }

        private static void updateCriteriaRange(ref OptionalRange<double> range, Operator op, double value, double tolerance = 0.05)
        {
            switch (op)
            {
                default:
                    return;

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
        }

        private static void updateCriteriaRange<T>(ref OptionalRange<T> range, Operator op, T value)
            where T : struct
        {
            switch (op)
            {
                default:
                    return;

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
        }

        protected enum Operator
        {
            Less,
            LessOrEqual,
            Equal,
            GreaterOrEqual,
            Greater
        }
    }
}
