// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public abstract class ManiaKeyMod : Mod, IApplicableToBeatmapConverter
    {
        public override string ShortenedName => Name;
        public abstract int KeyCount { get; }
        public override double ScoreMultiplier => 1; // TODO: Implement the mania key mod score multiplier
        public override bool Ranked => true;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var mbc = (ManiaBeatmapConverter)beatmapConverter;

            // Although this can work, for now let's not allow keymods for mania-specific beatmaps
            if (mbc.IsForCurrentRuleset)
                return;

            mbc.TargetColumns = KeyCount;
        }
    }
}
