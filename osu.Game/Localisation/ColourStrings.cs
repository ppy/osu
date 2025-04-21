// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// this file is meant for testing purposes when BindableColour4 is not availiable

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ColourStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Colour";

        /// <summary>
        /// "Red"
        /// </summary>
        public static LocalisableString Red => new TranslatableString(getKey(@"red"), @"Red");

        /// <summary>
        /// "Orange"
        /// </summary>
        public static LocalisableString Orange => new TranslatableString(getKey(@"orange"), @"Orange");

        /// <summary>
        /// "Yellow"
        /// </summary>
        public static LocalisableString Yellow => new TranslatableString(getKey(@"yellow"), @"Yellow");

        /// <summary>
        /// "Lime"
        /// </summary>
        public static LocalisableString Lime => new TranslatableString(getKey(@"lime"), @"Lime");

        /// <summary>
        /// "Green"
        /// </summary>
        public static LocalisableString Green => new TranslatableString(getKey(@"green"), @"Green");

        /// <summary>
        /// "Cyan"
        /// </summary>
        public static LocalisableString Cyan => new TranslatableString(getKey(@"cyan"), @"Cyan");

        /// <summary>
        /// "Light Blue"
        /// </summary>
        public static LocalisableString LightBlue => new TranslatableString(getKey(@"light_blue"), @"Light Blue");

        /// <summary>
        /// "Blue"
        /// </summary>
        public static LocalisableString Blue => new TranslatableString(getKey(@"blue"), @"Blue");

        /// <summary>
        /// "Purple"
        /// </summary>
        public static LocalisableString Purple => new TranslatableString(getKey(@"purple"), @"Purple");

        /// <summary>
        /// "Magenta"
        /// </summary>
        public static LocalisableString Magenta => new TranslatableString(getKey(@"magenta"), @"Magenta");

        /// <summary>
        /// "Pink"
        /// </summary>
        public static LocalisableString Pink => new TranslatableString(getKey(@"pink"), @"Pink");

        /// <summary>
        /// "White"
        /// </summary>
        public static LocalisableString White => new TranslatableString(getKey(@"white"), @"White");

        /// <summary>
        /// "Light Gray"
        /// </summary>
        public static LocalisableString LightGrey => new TranslatableString(getKey(@"light_grey"), @"Light Gray");

        /// <summary>
        /// "Gray"
        /// </summary>
        public static LocalisableString Grey => new TranslatableString(getKey(@"grey"), @"Gray");

        /// <summary>
        /// "Black"
        /// </summary>
        public static LocalisableString Black => new TranslatableString(getKey(@"black"), @"Black");

        /// <summary>
        /// "Brown"
        /// </summary>
        public static LocalisableString Brown => new TranslatableString(getKey(@"brown"), @"Brown");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
