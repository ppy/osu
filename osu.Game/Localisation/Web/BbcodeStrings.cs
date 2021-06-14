// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class BbcodeStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Bbcode";

        /// <summary>
        /// "Bold"
        /// </summary>
        public static LocalisableString Bold => new TranslatableString(getKey(@"bold"), @"Bold");

        /// <summary>
        /// "Header"
        /// </summary>
        public static LocalisableString Heading => new TranslatableString(getKey(@"heading"), @"Header");

        /// <summary>
        /// "Image"
        /// </summary>
        public static LocalisableString Image => new TranslatableString(getKey(@"image"), @"Image");

        /// <summary>
        /// "Italic"
        /// </summary>
        public static LocalisableString Italic => new TranslatableString(getKey(@"italic"), @"Italic");

        /// <summary>
        /// "Link"
        /// </summary>
        public static LocalisableString Link => new TranslatableString(getKey(@"link"), @"Link");

        /// <summary>
        /// "List"
        /// </summary>
        public static LocalisableString List => new TranslatableString(getKey(@"list"), @"List");

        /// <summary>
        /// "Numbered List"
        /// </summary>
        public static LocalisableString ListNumbered => new TranslatableString(getKey(@"list_numbered"), @"Numbered List");

        /// <summary>
        /// "Font Size"
        /// </summary>
        public static LocalisableString SizeDefault => new TranslatableString(getKey(@"size._"), @"Font Size");

        /// <summary>
        /// "Tiny"
        /// </summary>
        public static LocalisableString SizeTiny => new TranslatableString(getKey(@"size.tiny"), @"Tiny");

        /// <summary>
        /// "Small"
        /// </summary>
        public static LocalisableString SizeSmall => new TranslatableString(getKey(@"size.small"), @"Small");

        /// <summary>
        /// "Normal"
        /// </summary>
        public static LocalisableString SizeNormal => new TranslatableString(getKey(@"size.normal"), @"Normal");

        /// <summary>
        /// "Large"
        /// </summary>
        public static LocalisableString SizeLarge => new TranslatableString(getKey(@"size.large"), @"Large");

        /// <summary>
        /// "Spoiler Box"
        /// </summary>
        public static LocalisableString Spoilerbox => new TranslatableString(getKey(@"spoilerbox"), @"Spoiler Box");

        /// <summary>
        /// "Strike Out"
        /// </summary>
        public static LocalisableString Strikethrough => new TranslatableString(getKey(@"strikethrough"), @"Strike Out");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}