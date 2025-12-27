// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ScreenshotManagerStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ScreenshotManager";

        /// <summary>
        /// "Uploading screenshot..."
        /// </summary>
        public static LocalisableString UploadingScreenshot => new TranslatableString(getKey(@"uploading_screenshot"), @"Uploading screenshot...");

        /// <summary>
        /// "Screenshot uploaded! The link has been copied to the clipboard."
        /// </summary>
        public static LocalisableString UploadSuccess => new TranslatableString(getKey(@"upload_success"), @"Screenshot uploaded! The link has been copied to the clipboard.");

        /// <summary>
        /// "Failed to upload screenshot."
        /// </summary>
        public static LocalisableString UploadFailure => new TranslatableString(getKey(@"upload_failure"), @"Failed to upload screenshot.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
