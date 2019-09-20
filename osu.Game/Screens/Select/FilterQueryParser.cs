// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select
{
    internal static class FilterQueryParser
    {
        private static readonly Regex query_syntax_regex = new Regex(
            @"\b(?<key>stars|ar|dr|cs|divisor|length|objects|bpm|status)(?<op>[=:><]+)(?<value>\S*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static void ApplyQueries(FilterCriteria criteria, string query)
        {
            foreach (Match match in query_syntax_regex.Matches(query))
            {
                var key = match.Groups["key"].Value.ToLower();
                var op = match.Groups["op"].Value;
                var value = match.Groups["value"].Value;

                switch (key)
                {
                    case "stars" when float.TryParse(value, out var stars):
                        updateCriteriaRange(ref criteria.StarDifficulty, op, stars);
                        break;

                    case "ar" when float.TryParse(value, out var ar):
                        updateCriteriaRange(ref criteria.ApproachRate, op, ar);
                        break;

                    case "dr" when float.TryParse(value, out var dr):
                        updateCriteriaRange(ref criteria.DrainRate, op, dr);
                        break;

                    case "cs" when float.TryParse(value, out var cs):
                        updateCriteriaRange(ref criteria.CircleSize, op, cs);
                        break;

                    case "bpm" when double.TryParse(value, out var bpm):
                        updateCriteriaRange(ref criteria.BPM, op, bpm);
                        break;

                    case "length" when double.TryParse(value.TrimEnd('m', 's', 'h'), out var length):
                        var scale =
                            value.EndsWith("ms") ? 1 :
                            value.EndsWith("s") ? 1000 :
                            value.EndsWith("m") ? 60000 :
                            value.EndsWith("h") ? 3600000 : 1000;

                        updateCriteriaRange(ref criteria.Length, op, length * scale, scale / 2.0);
                        break;

                    case "divisor" when int.TryParse(value, out var divisor):
                        updateCriteriaRange(ref criteria.BeatDivisor, op, divisor);
                        break;

                    case "status" when Enum.TryParse<BeatmapSetOnlineStatus>(value, true, out var statusValue):
                        updateCriteriaRange(ref criteria.OnlineStatus, op, statusValue);
                        break;
                }

                query = query.Replace(match.ToString(), "");
            }

            criteria.SearchText = query;
        }

        private static void updateCriteriaRange(ref FilterCriteria.OptionalRange<float> range, string op, float value, float tolerance = 0.05f)
        {
            updateCriteriaRange(ref range, op, value);

            switch (op)
            {
                case "=":
                case ":":
                    range.Min = value - tolerance;
                    range.Max = value + tolerance;
                    break;
            }
        }

        private static void updateCriteriaRange(ref FilterCriteria.OptionalRange<double> range, string op, double value, double tolerance = 0.05)
        {
            updateCriteriaRange(ref range, op, value);

            switch (op)
            {
                case "=":
                case ":":
                    range.Min = value - tolerance;
                    range.Max = value + tolerance;
                    break;
            }
        }

        private static void updateCriteriaRange<T>(ref FilterCriteria.OptionalRange<T> range, string op, T value)
            where T : struct, IComparable
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
