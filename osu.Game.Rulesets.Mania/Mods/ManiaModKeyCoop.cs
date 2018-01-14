// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKeyCoop : Mod, IApplicableToBeatmapConverter<ManiaHitObject>
    {
        public override string Name => "KeyCoop";
        public override string ShortenedName => "2P";
        public override string Description => @"Double the key amount, double the fun!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;

        public void ApplyToBeatmapConverter(BeatmapConverter<ManiaHitObject> beatmapConverter)
        {
            var mbc = (ManiaBeatmapConverter)beatmapConverter;

            // Although this can work, for now let's not allow keymods for mania-specific beatmaps
            if (mbc.IsForCurrentRuleset)
                return;

            int originTargetColumns = mbc.TargetColumns;

            var newStages = new List<StageDefinition>()
            {
                new StageDefinition() { Columns = originTargetColumns },
                new StageDefinition() { Columns = originTargetColumns },
            };

            mbc.StageDefinitions = newStages;
            mbc.TargetColumns = originTargetColumns * 2;
        }
    }
}
