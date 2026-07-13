// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class CatchActionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.CatchAction";

        /// <summary>
        /// "Move left"
        /// </summary>
        public static LocalisableString MoveLeft => new TranslatableString(getKey(@"move_left"), @"Move left");

        /// <summary>
        /// "Move right"
        /// </summary>
        public static LocalisableString MoveRight => new TranslatableString(getKey(@"move_right"), @"Move right");

        /// <summary>
        /// "Engage dash"
        /// </summary>
        public static LocalisableString Dash => new TranslatableString(getKey(@"dash"), @"Engage dash");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
