// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModDualStages : Mod, IPlayfieldTypeMod, IApplicableToBeatmapConverter, IApplicableToRulesetContainer<ManiaHitObject>
    {
        public override string Name => "Dual Stages";
        public override string ShortenedName => "DS";
        public override string Description => @"Double the stages, double the fun!";
        public override double ScoreMultiplier => 0;

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

        public void ApplyToRulesetContainer(RulesetContainer<ManiaHitObject> rulesetContainer)
        {
            var mrc = (ManiaRulesetContainer)rulesetContainer;

            // Although this can work, for now let's not allow keymods for mania-specific beatmaps
            if (isForCurrentRuleset)
                return;

            var newDefinitions = new List<StageDefinition>();
            foreach (var existing in mrc.Beatmap.Stages)
            {
                newDefinitions.Add(new StageDefinition { Columns = existing.Columns / 2 });
                newDefinitions.Add(new StageDefinition { Columns = existing.Columns / 2 });
            }

            mrc.Beatmap.Stages = newDefinitions;
        }

        public PlayfieldType PlayfieldType => PlayfieldType.Dual;
    }
}
