// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.SkinComponents
{
    public static class BeatmapAttributeTextStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinComponents.BeatmapAttributeText";

        /// <summary>
        /// "Attribute"
        /// </summary>
        public static LocalisableString Attribute => new TranslatableString(getKey(@"attribute"), "Attribute");

        /// <summary>
        /// "The attribute to be displayed."
        /// </summary>
        public static LocalisableString AttributeDescription => new TranslatableString(getKey(@"attribute_description"), "The attribute to be displayed.");

        /// <summary>
        /// "Template"
        /// </summary>
        public static LocalisableString Template => new TranslatableString(getKey(@"template"), "Template");

        /// <summary>
        /// "Supports {{Label}} and {{Value}}, but also including arbitrary attributes like {{StarRating}} (see attribute list for supported values)."
        /// </summary>
        public static LocalisableString TemplateDescription => new TranslatableString(getKey(@"template_description"), @"Supports {{Label}} and {{Value}}, but also including arbitrary attributes like {{StarRating}} (see attribute list for supported values).");

        /// <summary>
        /// "Max Performance Point"
        /// </summary>
        public static LocalisableString MaxPerformancePoint => new TranslatableString(getKey(@"max_performance_point"), @"Max Performance Point");

        /// <summary>
        /// "Calculating..."
        /// </summary>
        public static LocalisableString PerformanceCalculating => new TranslatableString(getKey(@"performance_calculating"), @"Calculating...");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
