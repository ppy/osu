// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Objects
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
        /// MEH hit window at OD = 10.
        /// </summary>
        private const double meh_min = 242;
        /// <summary>
        /// MEH hit window at OD = 5.
        /// </summary>
        private const double meh_mid = 272;
        /// <summary>
        /// MEH hit window at OD = 0.
        /// </summary>
        private const double meh_max = 302;

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
        /// Hit window for a MEH hit.
        /// </summary>
        public double Meh = meh_mid;

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
            Meh = BeatmapDifficulty.DifficultyRange(difficulty, meh_max, meh_mid, meh_min);
            Miss = BeatmapDifficulty.DifficultyRange(difficulty, miss_max, miss_mid, miss_min);
        }

        /// <summary>
        /// Retrieves the hit result for a time offset.
        /// </summary>
        /// <param name="timeOffset">The time offset. This should always be a positive value indicating the absolute time offset.</param>
        /// <returns>The hit result, or null if <paramref name="timeOffset"/> doesn't result in a judgement.</returns>
        public HitResult? ResultFor(double timeOffset)
        {
            timeOffset = Math.Abs(timeOffset);

            if (timeOffset <= Perfect / 2)
                return HitResult.Perfect;
            if (timeOffset <= Great / 2)
                return HitResult.Great;
            if (timeOffset <= Good / 2)
                return HitResult.Good;
            if (timeOffset <= Ok / 2)
                return HitResult.Ok;
            if (timeOffset <= Meh / 2)
                return HitResult.Meh;
            if (timeOffset <= Miss / 2)
                return HitResult.Miss;

            return null;
        }

        /// <summary>
        /// Given a time offset, whether the <see cref="HitObject"/> can ever be hit in the future.
        /// This happens if <paramref name="timeOffset"/> > <see cref="Meh"/>.
        /// </summary>
        /// <param name="timeOffset">The time offset.</param>
        /// <returns>Whether the <see cref="HitObject"/> can be hit at any point in the future from this time offset.</returns>
        public bool CanBeHit(double timeOffset) => timeOffset <= Meh / 2;

        /// <summary>
        /// Multiplies all hit windows by a value.
        /// </summary>
        /// <param name="windows">The hit windows to multiply.</param>
        /// <param name="value">The value to multiply each hit window by.</param>
        public static HitWindows operator *(HitWindows windows, double value)
        {
            windows.Perfect *= value;
            windows.Great *= value;
            windows.Good *= value;
            windows.Ok *= value;
            windows.Meh *= value;
            windows.Miss *= value;

            return windows;
        }

        /// <summary>
        /// Divides all hit windows by a value.
        /// </summary>
        /// <param name="windows">The hit windows to divide.</param>
        /// <param name="value">The value to divide each hit window by.</param>
        public static HitWindows operator /(HitWindows windows, double value)
        {
            windows.Perfect /= value;
            windows.Great /= value;
            windows.Good /= value;
            windows.Ok /= value;
            windows.Meh /= value;
            windows.Miss /= value;

            return windows;
        }
    }
}
