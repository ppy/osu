// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
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
            string pattern = $@"^{getBaselineNameDetectingPattern(desiredName)}$";
            var regex = new Regex(pattern, RegexOptions.Compiled);

            int bestNumber = findBestNumber(existingNames, regex);

            return bestNumber == 0
                ? desiredName
                : $"{desiredName} ({bestNumber.ToString()})";
        }

        /// <summary>
        /// Given a set of <paramref name="existingFilenames"/> and a desired target <paramref name="desiredFilename"/>
        /// finds a filename closest to <paramref name="desiredFilename"/> that is not in <paramref name="existingFilenames"/>
        /// </summary>
        public static string GetNextBestFilename(IEnumerable<string> existingFilenames, string desiredFilename)
        {
            string name = Path.GetFileNameWithoutExtension(desiredFilename);
            string extension = Path.GetExtension(desiredFilename);

            string pattern = $@"^{getBaselineNameDetectingPattern(name)}(?i){Regex.Escape(extension)}(?-i)$";
            var regex = new Regex(pattern, RegexOptions.Compiled);

            int bestNumber = findBestNumber(existingFilenames, regex);

            return bestNumber == 0
                ? desiredFilename
                : $"{name} ({bestNumber.ToString()}){extension}";
        }

        /// <summary>
        /// Generates a basic regex pattern that will match all possible conflicting filenames when picking the best available name, given the <paramref name="desiredName"/>.
        /// The generated pattern can be composed into more complicated regexes for particular uses, such as picking filenames, which need additional file extension handling.
        /// </summary>
        /// <remarks>
        /// The regex shall detect:
        /// <list type="bullet">
        /// <item>all strings that are equal to <paramref name="desiredName"/>,</item>
        /// <item>all strings of the format <c>desiredName (number)</c>, where <c>number</c> is a number written using Arabic numerals.</item>
        /// </list>
        /// All comparisons are made in a case-insensitive manner.
        /// If a number is detected in the matches, it will be output to the <c>copyNumber</c> named group.
        /// </remarks>
        private static string getBaselineNameDetectingPattern(string desiredName)
            => $@"(?i){Regex.Escape(desiredName)}(?-i)( \((?<copyNumber>[1-9][0-9]*)\))?";

        private static int findBestNumber(IEnumerable<string> existingNames, Regex regex)
        {
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

            return bestNumber;
        }
    }
}
