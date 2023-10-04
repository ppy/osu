// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring.Legacy;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    internal class ManiaLegacyScoreSimulator : ILegacyScoreSimulator
    {
        public LegacyScoreAttributes Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap)
        {
            return new LegacyScoreAttributes { ComboScore = 1000000 };
        }
    }
}
