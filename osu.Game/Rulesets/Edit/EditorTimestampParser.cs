// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace osu.Game.Rulesets.Edit
{
    public static class EditorTimestampParser
    {
        /// <summary>
        /// Used for parsing in contexts where we don't want e.g. normal times of day to be parsed as timestamps (e.g. chat)
        /// Original osu-web regex:
        /// https://github.com/ppy/osu-web/blob/3b1698639244cfdaf0b41c68bfd651ea729ec2e3/resources/js/utils/beatmapset-discussion-helper.ts#L78
        /// </summary>
        /// <example>
        /// 00:00:000 (...) - test
        /// </example>
        public static readonly Regex TIME_REGEX_STRICT = new Regex(@"\b(((?<minutes>\d{2,}):(?<seconds>[0-5]\d)[:.](?<milliseconds>\d{3}))(?<selection>\s\([^)]+\))?)", RegexOptions.Compiled);

        /// <summary>
        /// Used for editor-specific context wherein we want to try as hard as we can to process user input as a timestamp.
        /// </summary>
        /// <example>
        /// <list type="bullet">
        /// <item>1 - parses to 00:00:001 (bare numbers are treated as milliseconds)</item>
        /// <item>1:2 - parses to 01:02:000</item>
        /// <item>1:02 - parses to 01:02:000</item>
        /// <item>1:92 - does not parse</item>
        /// <item>1:02:3 - parses to 01:02:003</item>
        /// <item>1:02:300 - parses to 01:02:300</item>
        /// <item>1:02:300 (1,2,3) - parses to 01:02:300 with selection</item>
        /// </list>
        /// </example>
        private static readonly Regex time_regex_lenient = new Regex(
            @"^(((?<minutes>\d{1,3}):(?<seconds>([0-5]?\d))([:.](?<milliseconds>\d{0,3}))?)(?<selection>\s\([^)]+\))?)(?<suffix>\s-.*)?$",
            RegexOptions.Compiled | RegexOptions.Singleline
        );

        public static bool TryParse(string timestamp, [NotNullWhen(true)] out TimeSpan? parsedTime, out string? parsedSelection)
        {
            if (double.TryParse(timestamp, out double msec))
            {
                parsedTime = TimeSpan.FromMilliseconds(msec);
                parsedSelection = null;
                return true;
            }

            Match match = time_regex_lenient.Match(timestamp);

            if (!match.Success)
            {
                parsedTime = null;
                parsedSelection = null;
                return false;
            }

            int timeMin, timeSec, timeMsec;

            int.TryParse(match.Groups[@"minutes"].Value, out timeMin);
            int.TryParse(match.Groups[@"seconds"].Value, out timeSec);
            int.TryParse(match.Groups[@"milliseconds"].Value, out timeMsec);

            // somewhat sane limit for timestamp duration (10 hours).
            if (timeMin >= 600)
            {
                parsedTime = null;
                parsedSelection = null;
                return false;
            }

            parsedTime = TimeSpan.FromMinutes(timeMin) + TimeSpan.FromSeconds(timeSec) + TimeSpan.FromMilliseconds(timeMsec);
            parsedSelection = match.Groups[@"selection"].Value.Trim();
            parsedSelection = !string.IsNullOrEmpty(parsedSelection) ? parsedSelection[1..^1] : null;
            return true;
        }
    }
}
