// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class BeatmapStatisticStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BeatmapStatisticStrings";

        /// <summary>
        /// "Circles"
        /// </summary>
        public static LocalisableString Circles => new TranslatableString(getKey(@"circles"), @"Circles");

        /// <summary>
        /// "Sliders"
        /// </summary>
        public static LocalisableString Sliders => new TranslatableString(getKey(@"sliders"), @"Sliders");

        /// <summary>
        /// "Spinners"
        /// </summary>
        public static LocalisableString Spinners => new TranslatableString(getKey(@"spinners"), @"Spinners");

        /// <summary>
        /// "Hits"
        /// </summary>
        public static LocalisableString Hits => new TranslatableString(getKey(@"hits"), @"Hits");

        /// <summary>
        /// "Drumrolls"
        /// </summary>
        public static LocalisableString Drumrolls => new TranslatableString(getKey(@"drumrolls"), @"Drumrolls");

        /// <summary>
        /// "Swells"
        /// </summary>
        public static LocalisableString Swells => new TranslatableString(getKey(@"swells"), @"Swells");

        /// <summary>
        /// "Fruits"
        /// </summary>
        public static LocalisableString Fruits => new TranslatableString(getKey(@"fruits"), @"Fruits");

        /// <summary>
        /// "Juice Streams"
        /// </summary>
        public static LocalisableString JuiceStreams => new TranslatableString(getKey(@"juice_streams"), @"Juice Streams");

        /// <summary>
        /// "Banana Showers"
        /// </summary>
        public static LocalisableString BananaShowers => new TranslatableString(getKey(@"banana_showers"), @"Banana Showers");

        /// <summary>
        /// "Notes"
        /// </summary>
        public static LocalisableString Notes => new TranslatableString(getKey(@"notes"), @"Notes");

        /// <summary>
        /// "Hold Notes"
        /// </summary>
        public static LocalisableString HoldNotes => new TranslatableString(getKey(@"hold_notes"), @"Hold Notes");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
