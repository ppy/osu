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
            @"\b(?<key>stars|ar|dr|hp|cs|divisor|length|objects|bpm|status|creator|artist)(?<op>[=:><]+)(?<value>("".*"")|(\S*))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private void applyQueries(string query)
        {
            foreach (Match match in query_syntax_regex.Matches(query))
            {
                var key = match.Groups["key"].Value.ToLower();
                var op = match.Groups["op"].Value;
                var value = match.Groups["value"].Value;

                parseKeywordCriteria(key, value, op);

                query = query.Replace(match.ToString(), "");
            }

            SearchText = query;
        }

        private void parseKeywordCriteria(string key, string value, string op)
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

        private static void updateCriteriaText(ref OptionalTextFilter textFilter, string op, string value)
        {
            switch (op)
            {
                case "=":
                case ":":
                    textFilter.SearchTerm = value.Trim('"');
                    break;
            }
        }

        private static void updateCriteriaRange(ref OptionalRange<float> range, string op, float value, float tolerance = 0.05f)
        {
            switch (op)
            {
                default:
                    return;

                case "=":
                case ":":
                    range.Min = value - tolerance;
                    range.Max = value + tolerance;
                    break;

                case ">":
                    range.Min = value + tolerance;
                    break;

                case ">=":
                case ">:":
                    range.Min = value - tolerance;
                    break;

                case "<":
                    range.Max = value - tolerance;
                    break;

                case "<=":
                case "<:":
                    range.Max = value + tolerance;
                    break;
            }
        }

        private static void updateCriteriaRange(ref OptionalRange<double> range, string op, double value, double tolerance = 0.05)
        {
            switch (op)
            {
                default:
                    return;

                case "=":
                case ":":
                    range.Min = value - tolerance;
                    range.Max = value + tolerance;
                    break;

                case ">":
                    range.Min = value + tolerance;
                    break;

                case ">=":
                case ">:":
                    range.Min = value - tolerance;
                    break;

                case "<":
                    range.Max = value - tolerance;
                    break;

                case "<=":
                case "<:":
                    range.Max = value + tolerance;
                    break;
            }
        }

        private static void updateCriteriaRange<T>(ref OptionalRange<T> range, string op, T value)
            where T : struct
        {
            switch (op)
            {
                default:
                    return;

                case "=":
                case ":":
                    range.IsLowerInclusive = range.IsUpperInclusive = true;
                    range.Min = value;
                    range.Max = value;
                    break;

                case ">":
                    range.IsLowerInclusive = false;
                    range.Min = value;
                    break;

                case ">=":
                case ">:":
                    range.IsLowerInclusive = true;
                    range.Min = value;
                    break;

                case "<":
                    range.IsUpperInclusive = false;
                    range.Max = value;
                    break;

                case "<=":
                case "<:":
                    range.IsUpperInclusive = true;
                    range.Max = value;
                    break;
            }
        }
    }
}
