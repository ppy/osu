// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Osu
{
    public static class OsuEditorSetupStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Osu.OsuEditorSetupStrings";

        /// <summary>
        /// "Stack Leniency"
        /// </summary>
        public static LocalisableString StackLeniency => new TranslatableString(getKey(@"stack_leniency"), @"Stack Leniency");

        /// <summary>
        /// "In play mode, osu! automatically stacks notes which occur at the same location. Increasing this value means it is more likely to snap notes of further time-distance."
        /// </summary>
        public static LocalisableString StackLeniencyDescription => new TranslatableString(getKey(@"stack_leniency_description"), @"In play mode, osu! automatically stacks notes which occur at the same location. Increasing this value means it is more likely to snap notes of further time-distance.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
