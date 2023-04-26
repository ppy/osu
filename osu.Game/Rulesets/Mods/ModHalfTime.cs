// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHalfTime : ModRateAdjust
    {
        public override string Name => "Half Time";
        public override string Acronym => "HT";
        public override IconUsage? Icon => OsuIcon.ModHalftime;
        public override ModType Type => ModType.DifficultyReduction;
        public override LocalisableString Description => "Less zoom...";

        [SettingSource("Speed decrease", "The actual decrease to apply")]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble(0.75)
        {
            MinValue = 0.5,
            MaxValue = 0.99,
            Precision = 0.01,
        };

        public override double ScoreMultiplier
        {
            get
            {
                // Round to the nearest multiple of 0.1.
                double value = (int)(SpeedChange.Value * 10) / 10.0;

                // Offset back to 0.
                value -= 1;

                return 1 + value;
            }
        }
    }
}
