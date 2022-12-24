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
        public static LocalisableString UserContentDisclaimerHeader => new TranslatableString(getKey(@"user_content_disclaimer"), @"User content disclaimer");

        /// <summary>
        /// "By turning off the &quot;Featured Artist&quot; filter, all user-uploaded content will be displayed.
        ///
        /// This includes content that may not be correctly licensed for osu! usage. Browse at your own risk."
        /// </summary>
        public static LocalisableString UserContentDisclaimerDescription => new TranslatableString(getKey(@"by_turning_off_the_featured"), @"By turning off the ""Featured Artist"" filter, all user-uploaded content will be displayed.

This includes content that may not be correctly licensed for osu! usage. Browse at your own risk.");

        /// <summary>
        /// "I understand"
        /// </summary>
        public static LocalisableString UserContentConfirmButtonText => new TranslatableString(getKey(@"understood"), @"I understand");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
