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

        private double speedMultiplier = 1;

        public double SpeedMultiplier
        {
            get => speedMultiplier;
            set
            {
                speedMultiplier = value;
                updateWindows();
            }
        }

        private double overallDifficulty;

        private bool useClassicWindows;

        public bool UseClassicWindows
        {
            get => useClassicWindows;
            set
            {
                useClassicWindows = value;
                updateWindows();
            }
        }

        private bool isConvert;

        public bool IsConvert
        {
            get => isConvert;
            set
            {
                isConvert = value;
                updateWindows();
            }
        }

        private double perfect;
        private double great;
        private double good;
        private double ok;
        private double meh;
        private double miss;

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
            overallDifficulty = difficulty;
            updateWindows();
        }

        private void updateWindows()
        {
            if (UseClassicWindows)
            {
                if (IsConvert)
                {
                    perfect = Math.Floor(16 * speedMultiplier) + 0.5;
                    great = Math.Floor((Math.Round(overallDifficulty) > 4 ? 34 : 47) * speedMultiplier) + 0.5;
                    good = Math.Floor((Math.Round(overallDifficulty) > 4 ? 67 : 77) * speedMultiplier) + 0.5;
                    ok = Math.Floor(97 * speedMultiplier) + 0.5;
                    meh = Math.Floor(121 * speedMultiplier) + 0.5;
                    miss = Math.Floor(158 * speedMultiplier) + 0.5;
                }
                else
                {
                    double invertedOd = Math.Clamp(10 - overallDifficulty, 0, 10);

                    perfect = Math.Floor(16 * speedMultiplier) + 0.5;
                    great = Math.Floor((34 + 3 * invertedOd) * speedMultiplier) + 0.5;
                    good = Math.Floor((67 + 3 * invertedOd) * speedMultiplier) + 0.5;
                    ok = Math.Floor((97 + 3 * invertedOd) * speedMultiplier) + 0.5;
                    meh = Math.Floor((121 + 3 * invertedOd) * speedMultiplier) + 0.5;
                    miss = Math.Floor((158 + 3 * invertedOd) * speedMultiplier) + 0.5;
                }
            }
            else
            {
                perfect = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, perfect_window_range) * speedMultiplier) + 0.5;
                great = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, great_window_range) * speedMultiplier) + 0.5;
                good = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, good_window_range) * speedMultiplier) + 0.5;
                ok = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, ok_window_range) * speedMultiplier) + 0.5;
                meh = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, meh_window_range) * speedMultiplier) + 0.5;
                miss = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, miss_window_range) * speedMultiplier) + 0.5;
            }
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
