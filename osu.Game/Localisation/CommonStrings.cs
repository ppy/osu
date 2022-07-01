// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class CommonStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Common";

        /// <summary>
        /// "Back"
        /// </summary>
        public static LocalisableString Back => new TranslatableString(getKey(@"back"), @"Back");

        /// <summary>
        /// "Next"
        /// </summary>
        public static LocalisableString Next => new TranslatableString(getKey(@"next"), @"Next");

        /// <summary>
        /// "Finish"
        /// </summary>
        public static LocalisableString Finish => new TranslatableString(getKey(@"finish"), @"Finish");

        /// <summary>
        /// "Enabled"
        /// </summary>
        public static LocalisableString Enabled => new TranslatableString(getKey(@"enabled"), @"Enabled");

        /// <summary>
        /// "Disabled"
        /// </summary>
        public static LocalisableString Disabled => new TranslatableString(getKey(@"disabled"), @"Disabled");

        /// <summary>
        /// "Default"
        /// </summary>
        public static LocalisableString Default => new TranslatableString(getKey(@"default"), @"Default");

        /// <summary>
        /// "Width"
        /// </summary>
        public static LocalisableString Width => new TranslatableString(getKey(@"width"), @"Width");

        /// <summary>
        /// "Height"
        /// </summary>
        public static LocalisableString Height => new TranslatableString(getKey(@"height"), @"Height");

        /// <summary>
        /// "Downloading..."
        /// </summary>
        public static LocalisableString Downloading => new TranslatableString(getKey(@"downloading"), @"Downloading...");

        /// <summary>
        /// "Importing..."
        /// </summary>
        public static LocalisableString Importing => new TranslatableString(getKey(@"importing"), @"Importing...");

        /// <summary>
        /// "Deselect All"
        /// </summary>
        public static LocalisableString DeselectAll => new TranslatableString(getKey(@"deselect_all"), @"Deselect All");

        /// <summary>
        /// "Select All"
        /// </summary>
        public static LocalisableString SelectAll => new TranslatableString(getKey(@"select_all"), @"Select All");

        /// <summary>
        /// "Beatmaps"
        /// </summary>
        public static LocalisableString Beatmaps => new TranslatableString(getKey(@"beatmaps"), @"Beatmaps");

        /// <summary>
        /// "Scores"
        /// </summary>
        public static LocalisableString Scores => new TranslatableString(getKey(@"scores"), @"Scores");

        /// <summary>
        /// "Skins"
        /// </summary>
        public static LocalisableString Skins => new TranslatableString(getKey(@"skins"), @"Skins");

        /// <summary>
        /// "Collections"
        /// </summary>
        public static LocalisableString Collections => new TranslatableString(getKey(@"collections"), @"Collections");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
