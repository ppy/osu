// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ScreenshotFormatStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ScreenshotFormat";

        /// <summary>
        /// "JPG (web-friendly)"
        /// </summary>
        public static LocalisableString JPGWebFriendly => new TranslatableString(getKey(@"jpgweb_friendly"), @"JPG (web-friendly)");

        /// <summary>
        /// "PNG (lossless)"
        /// </summary>
        public static LocalisableString PNGLossless => new TranslatableString(getKey(@"pnglossless"), @"PNG (lossless)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}