// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ManiaActionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ManiaAction";

        /// <summary>
        /// "Key {0}"
        /// </summary>
        public static LocalisableString Key(int keyNumber) => new TranslatableString(getKey(@"key"), @"Key {0}", keyNumber);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
