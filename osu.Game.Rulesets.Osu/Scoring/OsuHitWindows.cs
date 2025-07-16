// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuHitWindows : HitWindows
    {
        public static readonly DifficultyRange GREAT_WINDOW_RANGE = new DifficultyRange(80, 50, 20);
        public static readonly DifficultyRange OK_WINDOW_RANGE = new DifficultyRange(140, 100, 60);
        public static readonly DifficultyRange MEH_WINDOW_RANGE = new DifficultyRange(200, 150, 100);

        /// <summary>
        /// osu! ruleset has a fixed miss window regardless of difficulty settings.
        /// </summary>
        public const double MISS_WINDOW = 400;

        private double overallDifficulty;

        private bool classicModActive;

        public bool ClassicModActive
        {
            get => classicModActive;
            set
            {
                classicModActive = value;
                updateWindows();
            }
        }

        private double great;
        private double ok;
        private double meh;

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                case HitResult.Ok:
                case HitResult.Meh:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        public override void SetDifficulty(double difficulty)
        {
            overallDifficulty = difficulty;
            updateWindows();
        }

        private void updateWindows()
        {
            great = IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, GREAT_WINDOW_RANGE);
            ok = IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, OK_WINDOW_RANGE);
            meh = IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, MEH_WINDOW_RANGE);

            if (ClassicModActive)
            {
                great = Math.Floor(great) - 0.5;
                ok = Math.Floor(ok) - 0.5;
                meh = Math.Floor(meh) - 0.5;
            }
        }

        public override double WindowFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                    return great;

                case HitResult.Ok:
                    return ok;

                case HitResult.Meh:
                    return meh;

                case HitResult.Miss:
                    return MISS_WINDOW;

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}
