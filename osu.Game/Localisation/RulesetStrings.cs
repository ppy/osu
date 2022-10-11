// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RulesetStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Ruleset";

        /// <summary>
        /// "play some"
        /// </summary>

        public static LocalisableString HeaderDescription => new TranslatableString(getKey(@"header_description"), @"play some");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}