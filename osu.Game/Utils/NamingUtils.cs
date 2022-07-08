// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace osu.Game.Utils
{
    public static class NamingUtils
    {
        /// <summary>
        /// Given a set of <paramref name="existingNames"/> and a target <paramref name="desiredName"/>,
        /// finds a "best" name closest to <paramref name="desiredName"/> that is not in <paramref name="existingNames"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This helper is most useful in scenarios when creating new objects in a set
        /// (such as adding new difficulties to a beatmap set, or creating a clone of an existing object that needs a unique name).
        /// If <paramref name="desiredName"/> is already present in <paramref name="existingNames"/>,
        /// this method will append the lowest possible number in brackets that doesn't conflict with <paramref name="existingNames"/>
        /// to <paramref name="desiredName"/> and return that.
        /// See <c>osu.Game.Tests.Utils.NamingUtilsTest</c> for concrete examples of behaviour.
        /// </para>
        /// <para>
        /// <paramref name="desiredName"/> and <paramref name="existingNames"/> are compared in a case-insensitive manner,
        /// so this method is safe to use for naming files in a platform-invariant manner.
        /// </para>
        /// </remarks>
        public static string GetNextBestName(IEnumerable<string> existingNames, string desiredName)
        {
            string pattern = $@"^(?i){Regex.Escape(desiredName)}(?-i)( \((?<copyNumber>[1-9][0-9]*)\))?$";
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var takenNumbers = new HashSet<int>();

            foreach (string name in existingNames)
            {
                var match = regex.Match(name);
                if (!match.Success)
                    continue;

                string copyNumberString = match.Groups[@"copyNumber"].Value;

                if (string.IsNullOrEmpty(copyNumberString))
                {
                    takenNumbers.Add(0);
                    continue;
                }

                takenNumbers.Add(int.Parse(copyNumberString));
            }

            int bestNumber = 0;
            while (takenNumbers.Contains(bestNumber))
                bestNumber += 1;

            return bestNumber == 0
                ? desiredName
                : $"{desiredName} ({bestNumber})";
        }
    }
}
