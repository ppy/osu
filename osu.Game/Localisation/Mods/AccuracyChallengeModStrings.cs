// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class AccuracyChallengeModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.AccuracyChallengeMod";

        /// <summary>
        /// "Fail if your accuracy drops too low!"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "Fail if your accuracy drops too low!");

        /// <summary>
        /// "Minimum accuracy"
        /// </summary>
        public static LocalisableString MinAcc => new TranslatableString(getKey(@"min_acc"), "Minimum accuracy");

        /// <summary>
        /// "Trigger a failure if your accuracy foes below this value."
        /// </summary>
        public static LocalisableString MinAccDescription =>
            new TranslatableString(getKey(@"min_acc_description"), "Trigger a failure if your accuracy foes below this value.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
