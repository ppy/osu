// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    internal class ManiaLegacyScoreSimulator : ILegacyScoreSimulator
    {
        public int AccuracyScore => 0;
        public int ComboScore { get; private set; }
        public double BonusScoreRatio => 0;

        public void Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
        {
            double multiplier = mods.Where(m => m is not (ModHidden or ModHardRock or ModDoubleTime or ModFlashlight or ManiaModFadeIn))
                                    .Select(m => m.ScoreMultiplier)
                                    .Aggregate(1.0, (c, n) => c * n);

            ComboScore = (int)(1000000 * multiplier);
        }
    }
}
