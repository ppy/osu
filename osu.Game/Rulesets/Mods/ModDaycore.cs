// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDaycore : ModRateAdjust
    {
        public override string Name => "Daycore";
        public override string Acronym => "DC";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.DifficultyReduction;
        public override LocalisableString Description => "Whoaaaaa...";

        [SettingSource("Speed decrease", "The actual decrease to apply")]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble(0.75)
        {
            MinValue = 0.5,
            MaxValue = 0.99,
            Precision = 0.01,
        };

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        protected ModDaycore()
        {
            SpeedChange.BindValueChanged(val =>
            {
                freqAdjust.Value = SpeedChange.Default;
                tempoAdjust.Value = val.NewValue / SpeedChange.Default;
            }, true);
        }

        public override void ApplyToTrack(IAdjustableAudioComponent track)
        {
            // base.ApplyToTrack() intentionally not called (different tempo adjustment is applied)
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
        }

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
