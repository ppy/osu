// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select
{
    internal static class FilterQueryParser
    {
        //TODO Expression explanation
        private static readonly Regex query_syntax_regex = new Regex(
            @"((?<keyTextual>(status|creator|artist)\b)(?<opTextual>[=:])(?<valueTextual>("".*?"")|(\S*)))|((?<valuePreNumerical>([0-9]+\.?[0-9]*[mshdt]?))?(?<opPreNumerical>([=:><](?(?<=[><])([=:]?))))?(?<keyNumerical>(stars|ar|dr|cs|divisor|length|objects|bpm)\b)(?<opPostNumerical>([=:><](?(?<=[><])([=:]?))))?(?<valuePostNumerical>([0-9]+\.?[0-9]*[mshd]{0,2}))?)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static void ApplyQueries(FilterCriteria criteria, string query)
        {
            foreach (Match match in query_syntax_regex.Matches(query))
            {
                var keyTextual = match.Groups["keyTextual"].Value.ToLower();
                var opTextual = match.Groups["opTextual"].Value;
                var valueTextual = match.Groups["valueTextual"].Value;

                var keyNumerical = match.Groups["keyNumerical"].Value;
                //Operator used before the key needs to be inverted because when e.g. "4<stars<5"
                //is decomposited it actually means "stars<4" and "stars<5", but in order for 
                //the expression to be correct we need the first bit to be "stars>4".
                var opPreNumerical = invertPreNumericalOperator(match.Groups["opPreNumerical"].Value);
                var valuePreNumerical = match.Groups["valuePreNumerical"].Value;
                var opPostNumerical = match.Groups["opPostNumerical"].Value;
                var valuePostNumerical = match.Groups["valuePostNumerical"].Value;

                if (opTextual.Length != 0 || valueTextual.Length != 0)
                    parseKeywordCriteria(criteria, keyTextual, valueTextual, opTextual);

                if (opPreNumerical.Length != 0 || valuePreNumerical.Length != 0)
                    parseKeywordCriteria(criteria, keyNumerical, valuePreNumerical, opPreNumerical);

                if (opPostNumerical.Length != 0 || valuePostNumerical.Length != 0)
                    parseKeywordCriteria(criteria, keyNumerical, valuePostNumerical, opPostNumerical);

                query = query.Replace(match.ToString(), "");
            }

            criteria.SearchText = query;
        }

        private static string invertPreNumericalOperator(string op) {
            switch (op)
            {
                case "<":
                    return ">";
                case ">":
                    return "<";
                case "<=":
                    return ">=";
                case ">=":
                    return "<=";
                default:
                    return "";
            }
        }

        private static void parseKeywordCriteria(FilterCriteria criteria, string key, string value, string op)
        {
            switch (key)
            {
                case "stars" when parseFloatWithPoint(value, out var stars):
                    updateCriteriaRange(ref criteria.StarDifficulty, op, stars, 0.01f / 2);
                    break;

                case "ar" when parseFloatWithPoint(value, out var ar):
                    updateCriteriaRange(ref criteria.ApproachRate, op, ar, 0.1f / 2);
                    break;

                case "dr" when parseFloatWithPoint(value, out var dr):
                    updateCriteriaRange(ref criteria.DrainRate, op, dr, 0.1f / 2);
                    break;

                case "cs" when parseFloatWithPoint(value, out var cs):
                    updateCriteriaRange(ref criteria.CircleSize, op, cs, 0.1f / 2);
                    break;

                case "bpm" when parseDoubleWithPoint(value, out var bpm):
                    updateCriteriaRange(ref criteria.BPM, op, bpm, 0.01d / 2);
                    break;

                case "length" when parseDoubleWithPoint(value.TrimEnd('m', 's', 'h'), out var length):
                    var scale = getLengthScale(value);
                    updateCriteriaRange(ref criteria.Length, op, length * scale, scale / 2.0);
                    break;

                case "divisor" when parseInt(value, out var divisor):
                    updateCriteriaRange(ref criteria.BeatDivisor, op, divisor);
                    break;

                case "status" when Enum.TryParse<BeatmapSetOnlineStatus>(value, true, out var statusValue):
                    updateCriteriaRange(ref criteria.OnlineStatus, op, statusValue);
                    break;

                case "creator":
                    updateCriteriaText(ref criteria.Creator, op, value);
                    break;

                case "artist":
                    updateCriteriaText(ref criteria.Artist, op, value);
                    break;
            }
        }

        private static int getLengthScale(string value) =>
            value.EndsWith("ms") ? 1 :
            value.EndsWith("s") ? 1000 :
            value.EndsWith("m") ? 60000 :
            value.EndsWith("h") ? 3600000 : 1000;

        private static bool parseFloatWithPoint(string value, out float result) =>
            float.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);

        private static bool parseDoubleWithPoint(string value, out double result) =>
            double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);

        private static bool parseInt(string value, out int result) =>
            int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out result);

        private static void updateCriteriaText(ref FilterCriteria.OptionalTextFilter textFilter, string op, string value)
        {
            switch (op)
            {
                case "=":
                case ":":
                    textFilter.SearchTerm = value.Trim('"');
                    break;
            }
        }

        private static void updateCriteriaRange(ref FilterCriteria.OptionalRange<float> range, string op, float value, float tolerance = 0.05f)
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

        private static void updateCriteriaRange(ref FilterCriteria.OptionalRange<double> range, string op, double value, double tolerance = 0.05)
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

        private static void updateCriteriaRange<T>(ref FilterCriteria.OptionalRange<T> range, string op, T value)
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
