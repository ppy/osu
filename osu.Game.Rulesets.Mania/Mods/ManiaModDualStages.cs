// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModDualStages : Mod, IPlayfieldTypeMod, IApplicableToBeatmapConverter, IApplicableToBeatmap<ManiaHitObject>
    {
        public override string Name => "Dual Stages";
        public override string Acronym => "DS";
        public override string Description => @"Double the stages, double the fun!";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;

        private bool isForCurrentRuleset;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var mbc = (ManiaBeatmapConverter)beatmapConverter;

            isForCurrentRuleset = mbc.IsForCurrentRuleset;

            // Although this can work, for now let's not allow keymods for mania-specific beatmaps
            if (isForCurrentRuleset)
                return;

            mbc.TargetColumns *= 2;
        }

        public void ApplyToBeatmap(Beatmap<ManiaHitObject> beatmap)
        {
            if (isForCurrentRuleset)
                return;

            var maniaBeatmap = (ManiaBeatmap)beatmap;

            var newDefinitions = new List<StageDefinition>();
            foreach (var existing in maniaBeatmap.Stages)
            {
                newDefinitions.Add(new StageDefinition { Columns = existing.Columns / 2 });
                newDefinitions.Add(new StageDefinition { Columns = existing.Columns / 2 });
            }

            maniaBeatmap.Stages = newDefinitions;
        }

        public PlayfieldType PlayfieldType => PlayfieldType.Dual;
    }
}
