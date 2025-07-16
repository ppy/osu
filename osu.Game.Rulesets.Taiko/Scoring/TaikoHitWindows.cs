// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    public class TaikoHitWindows : HitWindows
    {
        public static readonly DifficultyRange GREAT_WINDOW_RANGE = new DifficultyRange(50, 35, 20);
        public static readonly DifficultyRange OK_WINDOW_RANGE = new DifficultyRange(120, 80, 50);
        public static readonly DifficultyRange MISS_WINDOW_RANGE = new DifficultyRange(135, 95, 70);

        private double great;
        private double ok;
        private double miss;

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

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                case HitResult.Ok:
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
            miss = IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, MISS_WINDOW_RANGE);

            if (ClassicModActive)
            {
                great = Math.Floor(great) - 0.5;
                ok = Math.Floor(ok) - 0.5;
                miss = Math.Floor(miss) - 0.5;
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

                case HitResult.Miss:
                    return miss;

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}
