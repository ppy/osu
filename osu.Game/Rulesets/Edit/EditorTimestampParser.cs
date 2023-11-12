// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace osu.Game.Rulesets.Edit
{
    public static class EditorTimestampParser
    {
        // 00:00:000 (...) - test
        // original osu-web regex: https://github.com/ppy/osu-web/blob/3b1698639244cfdaf0b41c68bfd651ea729ec2e3/resources/js/utils/beatmapset-discussion-helper.ts#L78
        public static readonly Regex TIME_REGEX = new Regex(@"\b(((\d{2,}):([0-5]\d)[:.](\d{3}))(\s\([^)]+\))?)");

        public static string[] GetRegexGroups(string timestamp)
        {
            Match match = TIME_REGEX.Match(timestamp);

            string[] result = match.Success
                ? match.Groups.Values.Where(x => x is not Match && !x.Value.Contains(':')).Select(x => x.Value).ToArray()
                : Array.Empty<string>();

            return result;
        }

        public static double GetTotalMilliseconds(params string[] timesGroup)
        {
            int[] times = timesGroup.Select(int.Parse).ToArray();

            Debug.Assert(times.Length == 3);

            return (times[0] * 60 + times[1]) * 1000 + times[2];
        }
    }
}
