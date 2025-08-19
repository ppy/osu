// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class RulesetActionsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RulesetActions";

        /// <summary>
        /// "Left button"
        /// </summary>
        public static LocalisableString OsuLeftButton => new TranslatableString(getKey(@"osu_left_button"), @"Left button");

        /// <summary>
        /// "Right button"
        /// </summary>
        public static LocalisableString OsuRightButton => new TranslatableString(getKey(@"osu_right_button"), @"Right button");

        /// <summary>
        /// "Smoke"
        /// </summary>
        public static LocalisableString OsuSmoke => new TranslatableString(getKey(@"osu_smoke"), @"Smoke");

        /// <summary>
        /// "Left (rim)"
        /// </summary>
        public static LocalisableString TaikoLeftRim => new TranslatableString(getKey(@"taiko_left_rim"), @"Left (rim)");

        /// <summary>
        /// "Left (centre)"
        /// </summary>
        public static LocalisableString TaikoLeftCentre => new TranslatableString(getKey(@"taiko_left_centre"), @"Left (centre)");

        /// <summary>
        /// "Right (centre)"
        /// </summary>
        public static LocalisableString TaikoRightCentre => new TranslatableString(getKey(@"taiko_right_centre"), @"Right (centre)");

        /// <summary>
        /// "Right (rim)"
        /// </summary>
        public static LocalisableString TaikoRightRim => new TranslatableString(getKey(@"taiko_right_rim"), @"Right (rim)");

        /// <summary>
        /// "Move left"
        /// </summary>
        public static LocalisableString CatchMoveLeft => new TranslatableString(getKey(@"taiko_move_left"), @"Move left");

        /// <summary>
        /// "Move right"
        /// </summary>
        public static LocalisableString CatchMoveRight => new TranslatableString(getKey(@"catch_move_right"), @"Move right");

        /// <summary>
        /// "Engage dash"
        /// </summary>
        public static LocalisableString CatchEngageDash => new TranslatableString(getKey(@"catch_engage_dash"), @"Engage dash");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
