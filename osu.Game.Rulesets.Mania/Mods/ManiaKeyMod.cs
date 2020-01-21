// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaKeyMod : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Keycount adjust";
        public override string Acronym => "XK";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1; // TODO: Implement the mania key mod score multiplier
        public override bool Ranked => true;

        [SettingSource("Key count", "Number of keys")]
        public Bindable<int> KeyCount { get; } = new BindableInt
        {
            MinValue = 1,
            MaxValue = 9,
            Default = 4
        };

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var mbc = (ManiaBeatmapConverter)beatmapConverter;

            // Although this can work, for now let's not allow keymods for mania-specific beatmaps
            if (mbc.IsForCurrentRuleset)
                return;

            mbc.TargetColumns = KeyCount.Value;
        }
    }
}
