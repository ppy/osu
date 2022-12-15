// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class BeatmapOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BeatmapOverlayStrings";

        /// <summary>
        /// "User content disclaimer"
        /// </summary>
        public static LocalisableString UserContentDisclaimer => new TranslatableString(getKey(@"user_content_disclaimer"), @"User content disclaimer");

        /// <summary>
        /// "By turning off the &quot;featured artist&quot; filter, all user uploaded content will be displayed.
        ///
        /// This includes content which may not be correctly licensed for use and as such may not be safe for streaming, sharing, or consumption."
        /// </summary>
        public static LocalisableString ByTurningOffTheFeatured => new TranslatableString(getKey(@"by_turning_off_the_featured"),
            @"By turning off the ""featured artist"" filter, all user uploaded content will be displayed.

This includes content which may not be correctly licensed for use and as such may not be safe for streaming, sharing, or consumption.");

        /// <summary>
        /// "I understand"
        /// </summary>
        public static LocalisableString Understood => new TranslatableString(getKey(@"understood"), @"I understand");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
