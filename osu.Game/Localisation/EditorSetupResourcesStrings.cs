// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorSetupResourcesStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorSetupResources";

        /// <summary>
        /// "Resources"
        /// </summary>
        public static LocalisableString Resources => new TranslatableString(getKey(@"resources"), @"Resources");

        /// <summary>
        /// "Audio Track"
        /// </summary>
        public static LocalisableString AudioTrack => new TranslatableString(getKey(@"audio_track"), @"Audio Track");

        /// <summary>
        /// "Click to select a track"
        /// </summary>
        public static LocalisableString ClickToSelectTrack => new TranslatableString(getKey(@"click_to_select_track"), @"Click to select a track");

        /// <summary>
        /// "Click to replace the track"
        /// </summary>
        public static LocalisableString ClickToReplaceTrack => new TranslatableString(getKey(@"click_to_replace_track"), @"Click to replace the track");

        /// <summary>
        /// "Click to select a background image"
        /// </summary>
        public static LocalisableString ClickToSelectBackground => new TranslatableString(getKey(@"click_to_select_background"), @"Click to select a background image");

        /// <summary>
        /// "Click to replace the background image"
        /// </summary>
        public static LocalisableString ClickToReplaceBackground => new TranslatableString(getKey(@"click_to_replace_background"), @"Click to replace the background image");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
