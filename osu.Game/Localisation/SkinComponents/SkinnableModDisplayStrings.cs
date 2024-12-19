// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.SkinComponents
{
    public static class SkinnableModDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinnableModDisplay";

        /// <summary>
        /// "Show extended information"
        /// </summary>
        public static LocalisableString ShowExtendedInformation => new TranslatableString(getKey(@"show_extended_information"), @"Show extended information");

        /// <summary>
        /// "Whether to show extended information for each mod."
        /// </summary>
        public static LocalisableString ShowExtendedInformationDescription => new TranslatableString(getKey(@"whether_to_show_extended_information"), @"Whether to show extended information for each mod.");

        /// <summary>
        /// "Expansion mode"
        /// </summary>
        public static LocalisableString ExpansionMode => new TranslatableString(getKey(@"expansion_mode"), @"Expansion mode");

        /// <summary>
        /// "How the mod display expands when interacted with."
        /// </summary>
        public static LocalisableString ExpansionModeDescription => new TranslatableString(getKey(@"how_the_mod_display_expands"), @"How the mod display expands when interacted with.");

        /// <summary>
        /// "Expand on hover"
        /// </summary>
        public static LocalisableString ExpandOnHover => new TranslatableString(getKey(@"expand_on_hover"), @"Expand on hover");

        /// <summary>
        /// "Always contracted"
        /// </summary>
        public static LocalisableString AlwaysContracted => new TranslatableString(getKey(@"always_contracted"), @"Always contracted");

        /// <summary>
        /// "Always expanded"
        /// </summary>
        public static LocalisableString AlwaysExpanded => new TranslatableString(getKey(@"always_expanded"), @"Always expanded");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
