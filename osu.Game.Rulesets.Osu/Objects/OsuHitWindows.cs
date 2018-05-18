// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class OsuHitWindows : HitWindows
    {
        private static readonly IReadOnlyDictionary<HitResult, (double od0, double od5, double od10)> base_ranges = new Dictionary<HitResult, (double, double, double)>
        {
            { HitResult.Great, (160, 100, 40) },
            { HitResult.Good, (280, 200, 120) },
            { HitResult.Meh, (400, 300, 200) },
            { HitResult.Miss, (400, 400, 400) },
        };

        public override void SetDifficulty(double difficulty)
        {
            Great = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Great]);
            Good = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Good]);
            Meh = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Meh]);
            Miss = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Miss]);
        }
    }
}
