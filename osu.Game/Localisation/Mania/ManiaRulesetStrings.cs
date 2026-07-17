// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mania
{
    public static class ManiaRulesetStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mania.ManiaRuleset";

        /// <summary>
        /// "Affects timing requirements for notes."
        /// </summary>
        public static LocalisableString AccuracyDescription => new TranslatableString(getKey(@"accuracy_description"), @"Affects timing requirements for notes.");

        /// <summary>
        /// "Affects the number of key columns on the playfield."
        /// </summary>
        public static LocalisableString KeyCountDescription => new TranslatableString(getKey(@"key_count_description"), @"Affects the number of key columns on the playfield.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
