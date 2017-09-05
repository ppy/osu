// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class HitWindows
    {
        #region Constants

        /// <summary>
        /// PERFECT hit window at OD = 10.
        /// </summary>
        private const double perfect_min = 27.8;
        /// <summary>
        /// PERFECT hit window at OD = 5.
        /// </summary>
        private const double perfect_mid = 38.8;
        /// <summary>
        /// PERFECT hit window at OD = 0.
        /// </summary>
        private const double perfect_max = 44.8;

        /// <summary>
        /// GREAT hit window at OD = 10.
        /// </summary>
        private const double great_min = 68;
        /// <summary>
        /// GREAT hit window at OD = 5.
        /// </summary>
        private const double great_mid = 98;
        /// <summary>
        /// GREAT hit window at OD = 0.
        /// </summary>
        private const double great_max = 128;

        /// <summary>
        /// GOOD hit window at OD = 10.
        /// </summary>
        private const double good_min = 134;
        /// <summary>
        /// GOOD hit window at OD = 5.
        /// </summary>
        private const double good_mid = 164;
        /// <summary>
        /// GOOD hit window at OD = 0.
        /// </summary>
        private const double good_max = 194;

        /// <summary>
        /// OK hit window at OD = 10.
        /// </summary>
        private const double ok_min = 194;
        /// <summary>
        /// OK hit window at OD = 5.
        /// </summary>
        private const double ok_mid = 224;
        /// <summary>
        /// OK hit window at OD = 0.
        /// </summary>
        private const double ok_max = 254;

        /// <summary>
        /// BAD hit window at OD = 10.
        /// </summary>
        private const double bad_min = 242;
        /// <summary>
        /// BAD hit window at OD = 5.
        /// </summary>
        private const double bad_mid = 272;
        /// <summary>
        /// BAD hit window at OD = 0.
        /// </summary>
        private const double bad_max = 302;

        /// <summary>
        /// MISS hit window at OD = 10.
        /// </summary>
        private const double miss_min = 316;
        /// <summary>
        /// MISS hit window at OD = 5.
        /// </summary>
        private const double miss_mid = 346;
        /// <summary>
        /// MISS hit window at OD = 0.
        /// </summary>
        private const double miss_max = 376;

        #endregion

        /// <summary>
        /// Hit window for a PERFECT hit.
        /// </summary>
        public double Perfect = perfect_mid;

        /// <summary>
        /// Hit window for a GREAT hit.
        /// </summary>
        public double Great = great_mid;

        /// <summary>
        /// Hit window for a GOOD hit.
        /// </summary>
        public double Good = good_mid;

        /// <summary>
        /// Hit window for an OK hit.
        /// </summary>
        public double Ok = ok_mid;

        /// <summary>
        /// Hit window for a BAD hit.
        /// </summary>
        public double Bad = bad_mid;

        /// <summary>
        /// Hit window for a MISS hit.
        /// </summary>
        public double Miss = miss_mid;

        /// <summary>
        /// Constructs default hit windows.
        /// </summary>
        public HitWindows()
        {
        }

        /// <summary>
        /// Constructs hit windows by fitting a parameter to a 2-part piecewise linear function for each hit window.
        /// </summary>
        /// <param name="difficulty">The parameter.</param>
        public HitWindows(double difficulty)
        {
            Perfect = BeatmapDifficulty.DifficultyRange(difficulty, perfect_max, perfect_mid, perfect_min);
            Great = BeatmapDifficulty.DifficultyRange(difficulty, great_max, great_mid, great_min);
            Good = BeatmapDifficulty.DifficultyRange(difficulty, good_max, good_mid, good_min);
            Ok = BeatmapDifficulty.DifficultyRange(difficulty, ok_max, ok_mid, ok_min);
            Bad = BeatmapDifficulty.DifficultyRange(difficulty, bad_max, bad_mid, bad_min);
            Miss = BeatmapDifficulty.DifficultyRange(difficulty, miss_max, miss_mid, miss_min);
        }

        /// <summary>
        /// Retrieves the hit result for a time offset.
        /// </summary>
        /// <param name="hitOffset">The time offset.</param>
        /// <returns>The hit result, or null if the time offset results in a miss.</returns>
        public HitResult? ResultFor(double hitOffset)
        {
            if (hitOffset <= Perfect / 2)
                return HitResult.Perfect;
            if (hitOffset <= Great / 2)
                return HitResult.Great;
            if (hitOffset <= Good / 2)
                return HitResult.Good;
            if (hitOffset <= Ok / 2)
                return HitResult.Ok;
            if (hitOffset <= Bad / 2)
                return HitResult.Meh;
            return null;
        }

        /// <summary>
        /// Constructs new hit windows which have been multiplied by a value.
        /// </summary>
        /// <param name="windows">The original hit windows.</param>
        /// <param name="value">The value to multiply each hit window by.</param>
        public static HitWindows operator *(HitWindows windows, double value)
        {
            return new HitWindows
            {
                Perfect = windows.Perfect * value,
                Great = windows.Great * value,
                Good = windows.Good * value,
                Ok = windows.Ok * value,
                Bad = windows.Bad * value,
                Miss = windows.Miss * value
            };
        }

        /// <summary>
        /// Constructs new hit windows which have been divided by a value.
        /// </summary>
        /// <param name="windows">The original hit windows.</param>
        /// <param name="value">The value to divide each hit window by.</param>
        public static HitWindows operator /(HitWindows windows, double value)
        {
            return new HitWindows
            {
                Perfect = windows.Perfect / value,
                Great = windows.Great / value,
                Good = windows.Good / value,
                Ok = windows.Ok / value,
                Bad = windows.Bad / value,
                Miss = windows.Miss / value
            };
        }
    }
}
