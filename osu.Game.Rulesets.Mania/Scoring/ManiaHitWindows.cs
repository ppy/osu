// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public class ManiaHitWindows : HitWindows
    {
        private static readonly DifficultyRange perfect_window_range = new DifficultyRange(22.4D, 19.4D, 13.9D);
        private static readonly DifficultyRange great_window_range = new DifficultyRange(64, 49, 34);
        private static readonly DifficultyRange good_window_range = new DifficultyRange(97, 82, 67);
        private static readonly DifficultyRange ok_window_range = new DifficultyRange(127, 112, 97);
        private static readonly DifficultyRange meh_window_range = new DifficultyRange(151, 136, 121);
        private static readonly DifficultyRange miss_window_range = new DifficultyRange(188, 173, 158);

        private readonly double multiplier;

        private double perfect;
        private double great;
        private double good;
        private double ok;
        private double meh;
        private double miss;

        public ManiaHitWindows()
            : this(1)
        {
        }

        public ManiaHitWindows(double multiplier)
        {
            this.multiplier = multiplier;
        }

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Ok:
                case HitResult.Meh:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        public override void SetDifficulty(double difficulty)
        {
            perfect = IBeatmapDifficultyInfo.DifficultyRange(difficulty, perfect_window_range) * multiplier;
            great = IBeatmapDifficultyInfo.DifficultyRange(difficulty, great_window_range) * multiplier;
            good = IBeatmapDifficultyInfo.DifficultyRange(difficulty, good_window_range) * multiplier;
            ok = IBeatmapDifficultyInfo.DifficultyRange(difficulty, ok_window_range) * multiplier;
            meh = IBeatmapDifficultyInfo.DifficultyRange(difficulty, meh_window_range) * multiplier;
            miss = IBeatmapDifficultyInfo.DifficultyRange(difficulty, miss_window_range) * multiplier;
        }

        public override double WindowFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                    return perfect;

                case HitResult.Great:
                    return great;

                case HitResult.Good:
                    return good;

                case HitResult.Ok:
                    return ok;

                case HitResult.Meh:
                    return meh;

                case HitResult.Miss:
                    return miss;

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}
