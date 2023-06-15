// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    internal class ManiaScoreV1Processor
    {
        public int TotalScore { get; private set; }

        public ManiaScoreV1Processor(IReadOnlyList<Mod> mods)
        {
            double multiplier = mods.Where(m => m is not (ModHidden or ModHardRock or ModDoubleTime or ModFlashlight or ManiaModFadeIn))
                                    .Select(m => m.ScoreMultiplier)
                                    .Aggregate(1.0, (c, n) => c * n);

            TotalScore = (int)(1000000 * multiplier);
        }
    }
}
