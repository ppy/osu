// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RandomSelectAlgorithmStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RandomSelectAlgorithm";

        /// <summary>
        /// "Never repeat"
        /// </summary>
        public static LocalisableString NeverRepeat => new TranslatableString(getKey(@"never_repeat"), @"Never repeat");

        /// <summary>
        /// "True Random"
        /// </summary>
        public static LocalisableString TrueRandom => new TranslatableString(getKey(@"true_random"), @"True Random");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}