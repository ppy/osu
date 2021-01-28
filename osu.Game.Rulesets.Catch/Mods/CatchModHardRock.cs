// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHardRock : ModHardRock, IApplicableToBeatmap
    {
        public override double ScoreMultiplier => 1.12;
        public override bool Ranked => true;

        public void ApplyToBeatmap(IBeatmap beatmap) => CatchBeatmapProcessor.ApplyPositionOffsets(beatmap, this);
    }
}
