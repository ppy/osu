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

        /// <summary>
        /// "Key 1"
        /// </summary>
        public static LocalisableString ManiaKey1 => new TranslatableString(getKey(@"mania_key1"), @"Key 1");

        /// <summary>
        /// "Key 2"
        /// </summary>
        public static LocalisableString ManiaKey2 => new TranslatableString(getKey(@"mania_key2"), @"Key 2");

        /// <summary>
        /// "Key 3"
        /// </summary>
        public static LocalisableString ManiaKey3 => new TranslatableString(getKey(@"mania_key3"), @"Key 3");

        /// <summary>
        /// "Key 4"
        /// </summary>
        public static LocalisableString ManiaKey4 => new TranslatableString(getKey(@"mania_key4"), @"Key 4");

        /// <summary>
        /// "Key 5"
        /// </summary>
        public static LocalisableString ManiaKey5 => new TranslatableString(getKey(@"mania_key5"), @"Key 5");

        /// <summary>
        /// "Key 6"
        /// </summary>
        public static LocalisableString ManiaKey6 => new TranslatableString(getKey(@"mania_key6"), @"Key 6");

        /// <summary>
        /// "Key 7"
        /// </summary>
        public static LocalisableString ManiaKey7 => new TranslatableString(getKey(@"mania_key7"), @"Key 7");

        /// <summary>
        /// "Key 8"
        /// </summary>
        public static LocalisableString ManiaKey8 => new TranslatableString(getKey(@"mania_key8"), @"Key 8");

        /// <summary>
        /// "Key 9"
        /// </summary>
        public static LocalisableString ManiaKey9 => new TranslatableString(getKey(@"mania_key9"), @"Key 9");

        /// <summary>
        /// "Key 10"
        /// </summary>
        public static LocalisableString ManiaKey10 => new TranslatableString(getKey(@"mania_key10"), @"Key 10");

        /// <summary>
        /// "Key 11"
        /// </summary>
        public static LocalisableString ManiaKey11 => new TranslatableString(getKey(@"mania_key11"), @"Key 11");

        /// <summary>
        /// "Key 12"
        /// </summary>
        public static LocalisableString ManiaKey12 => new TranslatableString(getKey(@"mania_key12"), @"Key 12");

        /// <summary>
        /// "Key 13"
        /// </summary>
        public static LocalisableString ManiaKey13 => new TranslatableString(getKey(@"mania_key13"), @"Key 13");

        /// <summary>
        /// "Key 14"
        /// </summary>
        public static LocalisableString ManiaKey14 => new TranslatableString(getKey(@"mania_key14"), @"Key 14");

        /// <summary>
        /// "Key 15"
        /// </summary>
        public static LocalisableString ManiaKey15 => new TranslatableString(getKey(@"mania_key15"), @"Key 15");

        /// <summary>
        /// "Key 16"
        /// </summary>
        public static LocalisableString ManiaKey16 => new TranslatableString(getKey(@"mania_key16"), @"Key 16");

        /// <summary>
        /// "Key 17"
        /// </summary>
        public static LocalisableString ManiaKey17 => new TranslatableString(getKey(@"mania_key17"), @"Key 17");

        /// <summary>
        /// "Key 18"
        /// </summary>
        public static LocalisableString ManiaKey18 => new TranslatableString(getKey(@"mania_key18"), @"Key 18");

        /// <summary>
        /// "Key 19"
        /// </summary>
        public static LocalisableString ManiaKey19 => new TranslatableString(getKey(@"mania_key19"), @"Key 19");

        /// <summary>
        /// "Key 20"
        /// </summary>
        public static LocalisableString ManiaKey20 => new TranslatableString(getKey(@"mania_key20"), @"Key 20");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
