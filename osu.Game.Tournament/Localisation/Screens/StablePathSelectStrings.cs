// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class StablePathSelectStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Screens.StablePathSelect";

        /// <summary>
        /// "Please select a new location"
        /// </summary>
        public static LocalisableString PathSelectTitle => new TranslatableString(getKey(@"path_select_title"), @"Please select a new location");

        /// <summary>
        /// "Select stable path"
        /// </summary>
        public static LocalisableString SelectStablePath => new TranslatableString(getKey(@"select_stable_path"), @"Select stable path");

        /// <summary>
        /// "Auto detect"
        /// </summary>
        public static LocalisableString AutoDetect => new TranslatableString(getKey(@"auto_detect"), @"Auto detect");

        /// <summary>
        /// "This is an invalid IPC Directory"
        /// </summary>
        public static LocalisableString InvalidDirectoryTitle => new TranslatableString(getKey(@"invalid_directory_title"), @"This is an invalid IPC Directory");

        /// <summary>
        /// "Select a directory that contains an osu! stable cutting edge installation and make sure it has an empty ipc.txt file in it."
        /// </summary>
        public static LocalisableString InvalidDirectoryText => new TranslatableString(getKey(@"invalid_directory_text"),
            @"Select a directory that contains an osu! stable cutting edge installation and make sure it has an empty ipc.txt file in it.");

        /// <summary>
        /// "Failed to automatically detect"
        /// </summary>
        public static LocalisableString DetectFailureTitle => new TranslatableString(getKey(@"detect_failure_title"), @"Failed to automatically detect");

        /// <summary>
        /// "An osu! stable cutting-edge installation could not be detected. Please try and manually point to the directory."
        /// </summary>
        public static LocalisableString DetectFailureText => new TranslatableString(getKey(@"detect_failure_text"),
            "An osu! stable cutting-edge installation could not be detected.\nPlease try and manually point to the directory.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

