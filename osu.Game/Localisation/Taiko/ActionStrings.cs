// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Taiko
{
    public static class ActionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Taiko.Action";

        /// <summary>
        /// "Left (rim)"
        /// </summary>
        public static LocalisableString LeftRim => new TranslatableString(getKey(@"left_rim"), @"Left (rim)");

        /// <summary>
        /// "Left (centre)"
        /// </summary>
        public static LocalisableString LeftCentre => new TranslatableString(getKey(@"left_centre"), @"Left (centre)");

        /// <summary>
        /// "Right (centre)"
        /// </summary>
        public static LocalisableString RightCentre => new TranslatableString(getKey(@"right_centre"), @"Right (centre)");

        /// <summary>
        /// "Right (rim)"
        /// </summary>
        public static LocalisableString RightRim => new TranslatableString(getKey(@"right_rim"), @"Right (rim)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
