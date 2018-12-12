// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class TaikoHitWindows : HitWindows
    {
        private static readonly IReadOnlyDictionary<HitResult, (double od0, double od5, double od10)> base_ranges = new Dictionary<HitResult, (double, double, double)>
        {
            { HitResult.Great, (100, 70, 40) },
            { HitResult.Good, (240, 160, 100) },
            { HitResult.Miss, (270, 190, 140) },
        };

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Miss:
                    return true;
                default:
                    return false;
            }
        }

        public override void SetDifficulty(double difficulty)
        {
            Great = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Great]);
            Good = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Good]);
            Miss = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Miss]);
        }
    }
}
