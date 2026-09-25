// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.SkinComponents
{
    public static class FlowKeyCounterDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.FlowKeyCounterDisplay";

        /// <summary>
        /// "Show Key Name"
        /// </summary>
        public static LocalisableString ShowKeyName => new TranslatableString(getKey(@"show_key_name"), @"Show Key Name");

        /// <summary>
        /// "Flow Duration"
        /// </summary>
        public static LocalisableString FlowDuration => new TranslatableString(getKey(@"flow_duration"), @"Flow Duration");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
