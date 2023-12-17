// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaKeyMod : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Key Count";
        public override string Acronym => "XK";
        public override string ExtendedIconInformation => $"{KeyCount.Value}K";
        public override ModType Type => ModType.Conversion;
        public override LocalisableString Description => @"Play with a different amount of keys.";
        public override string SettingDescription => "keys".ToQuantity(KeyCount.Value);
        public override double ScoreMultiplier => 1; // TODO: Implement the mania key mod score multiplier

        [SettingSource("Key count", "Number of keys")]
        public Bindable<int> KeyCount { get; } = new BindableInt(4)
        {
            MinValue = 1,
            MaxValue = 10,
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
