// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace osu.Game.Rulesets.Edit
{
    public static class EditorTimestampParser
    {
        // 00:00:000 (...) - test
        // original osu-web regex: https://github.com/ppy/osu-web/blob/3b1698639244cfdaf0b41c68bfd651ea729ec2e3/resources/js/utils/beatmapset-discussion-helper.ts#L78
        public static readonly Regex TIME_REGEX = new Regex(@"\b(((?<minutes>\d{2,}):(?<seconds>[0-5]\d)[:.](?<milliseconds>\d{3}))(?<selection>\s\([^)]+\))?)", RegexOptions.Compiled);

        public static bool TryParse(string timestamp, [NotNullWhen(true)] out TimeSpan? parsedTime, out string? parsedSelection)
        {
            Match match = TIME_REGEX.Match(timestamp);

            if (!match.Success)
            {
                parsedTime = null;
                parsedSelection = null;
                return false;
            }

            bool result = true;

            result &= int.TryParse(match.Groups[@"minutes"].Value, out int timeMin);
            result &= int.TryParse(match.Groups[@"seconds"].Value, out int timeSec);
            result &= int.TryParse(match.Groups[@"milliseconds"].Value, out int timeMsec);

            // somewhat sane limit for timestamp duration (10 hours).
            result &= timeMin < 600;

            if (!result)
            {
                parsedTime = null;
                parsedSelection = null;
                return false;
            }

            parsedTime = TimeSpan.FromMinutes(timeMin) + TimeSpan.FromSeconds(timeSec) + TimeSpan.FromMilliseconds(timeMsec);
            parsedSelection = match.Groups[@"selection"].Value.Trim();
            if (!string.IsNullOrEmpty(parsedSelection))
                parsedSelection = parsedSelection[1..^1];
            return true;
        }
    }
}
