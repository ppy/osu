// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class FailConditionModsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.FailConditionMods";

        /// <summary>
        /// "Restart on fail"
        /// </summary>
        public static LocalisableString Restart => new TranslatableString(getKey(@"restart"), "Restart on fail");

        /// <summary>
        /// "Automatically restarts when failed."
        /// </summary>
        public static LocalisableString RestartDescription => new TranslatableString(getKey(@"restart_description"), "Automatically restarts when failed.");

        /// <summary>
        /// "Miss and fail."
        /// </summary>
        public static LocalisableString SuddenDeathDescription => new TranslatableString(getKey(@"sudden_death_description"), "Miss and fail.");

        /// <summary>
        /// "SS or quit."
        /// </summary>
        public static LocalisableString PerfectDescription => new TranslatableString(getKey(@"perfect_description"), "SS or quit.");

        /// <summary>
        /// "Fail if your accuracy drops too low!"
        /// </summary>
        public static LocalisableString AccuracyChallengeDescription => new TranslatableString(getKey(@"accuracy_challenge_description"), "Fail if your accuracy drops too low!");

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
