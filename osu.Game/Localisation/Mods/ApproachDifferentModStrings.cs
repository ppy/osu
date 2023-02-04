// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class ApproachDifferentModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.ApproachDifferentMod";

        /// <summary>
        /// "Never trust the approach circles..."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "Never trust the approach circles...");

        /// <summary>
        /// "Initial size"
        /// </summary>
        public static LocalisableString Scale => new TranslatableString(getKey(@"scale"), "Initial size");

        /// <summary>
        /// "Change the initial size of the approach circle, relative to hit circles."
        /// </summary>
        public static LocalisableString ScaleDescription => new TranslatableString(getKey(@"scale_description"), "Change the initial size of the approach circle, relative to hit circles.");

        /// <summary>
        /// "Style"
        /// </summary>
        public static LocalisableString Style => new TranslatableString(getKey(@"style"), "Style");

        /// <summary>
        /// "Change the animation style of the approach circles."
        /// </summary>
        public static LocalisableString StyleDescription => new TranslatableString(getKey(@"style_description"), "Change the animation style of the approach circles.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
