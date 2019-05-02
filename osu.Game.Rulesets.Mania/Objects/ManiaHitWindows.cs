// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects
{
    public class ManiaHitWindows : HitWindows
    {
        private static readonly IReadOnlyDictionary<HitResult, (double od0, double od5, double od10)> base_ranges = new Dictionary<HitResult, (double, double, double)>
        {
            { HitResult.Perfect, (44.8, 38.8, 27.8) },
            { HitResult.Great, (128, 98, 68) },
            { HitResult.Good, (194, 164, 134) },
            { HitResult.Ok, (254, 224, 194) },
            { HitResult.Meh, (302, 272, 242) },
            { HitResult.Miss, (376, 346, 316) },
        };

        public override bool IsHitResultAllowed(HitResult result) => true;

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
