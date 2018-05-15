// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    public class ConvertHitWindows : HitWindows
    {
        private static readonly IReadOnlyDictionary<HitResult, (double od0, double od5, double od10)> base_ranges = new Dictionary<HitResult, (double, double, double)>
        {
            { HitResult.Perfect, (44.8, 38.8, 27.8) },
            { HitResult.Great, (128, 98, 68 ) },
            { HitResult.Good, (194, 164, 134) },
            { HitResult.Ok, (254, 224, 194) },
            { HitResult.Meh, (302, 272, 242) },
            { HitResult.Miss, (376, 346, 316) },
        };

        public override void SetDifficulty(double difficulty)
        {
            Perfect = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Perfect]);
            Great = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Great]);
            Good = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Good]);
            Ok = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Ok]);
            Meh = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Meh]);
            Miss = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Miss]);
        }
    }
}
