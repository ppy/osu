// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SkinEditorStrings
    {
        private const string prefix = "osu.Game.Localisation.SkinEditor";

        /// <summary>
        /// "anchor"
        /// </summary>
        public static LocalisableString Anchor => new TranslatableString(getKey("anchor"), "anchor");

        /// <summary>
        /// "origin"
        /// </summary>
        public static LocalisableString Origin => new TranslatableString(getKey("origin"), "origin");

        /// <summary>
        /// "top-left"
        /// </summary>
        public static LocalisableString TopLeft => new TranslatableString(getKey("top_left"), "top-left");

        /// <summary>
        /// "top-centre"
        /// </summary>
        public static LocalisableString TopCentre => new TranslatableString(getKey("top_centre"), "top-centre");

        /// <summary>
        /// "top-right"
        /// </summary>
        public static LocalisableString TopRight => new TranslatableString(getKey("top_right"), "top-right");

        /// <summary>
        /// "centre-left"
        /// </summary>
        public static LocalisableString CentreLeft => new TranslatableString(getKey("centre_left"), "centre-left");

        /// <summary>
        /// "centre"
        /// </summary>
        public static LocalisableString Centre => new TranslatableString(getKey("centre"), "centre");

        /// <summary>
        /// "centre-right"
        /// </summary>
        public static LocalisableString CentreRight => new TranslatableString(getKey("centre_right"), "centre-right");

        /// <summary>
        /// "bottom-left"
        /// </summary>
        public static LocalisableString BottomLeft => new TranslatableString(getKey("bottom_left"), "bottom-left");

        /// <summary>
        /// "bottom-centre"
        /// </summary>
        public static LocalisableString BottomCentre => new TranslatableString(getKey("bottom_centre"), "bottom-centre");

        /// <summary>
        /// "bottom-right"
        /// </summary>
        public static LocalisableString BottomRight => new TranslatableString(getKey("bottom_right"), "bottom-right");

        /// <summary>
        /// "closest"
        /// </summary>
        public static LocalisableString Closest => new TranslatableString(getKey("closest"), "closest");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
