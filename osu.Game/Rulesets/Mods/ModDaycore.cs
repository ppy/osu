// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDaycore : ModRateAdjust
    {
        public override string Name => "Daycore";
        public override string Acronym => "DC";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.DifficultyReduction;
        public override LocalisableString Description => "Whoaaaaa...";
        public override bool EligibleForPP => UsesDefaultConfiguration;

        [SettingSource("Speed decrease", "The actual decrease to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble(0.75)
        {
            MinValue = 0.5,
            MaxValue = 0.99,
            Precision = 0.01,
        };

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);
        private readonly RateAdjustModHelper rateAdjustHelper;

        protected ModDaycore()
        {
            rateAdjustHelper = new RateAdjustModHelper(SpeedChange);

            // intentionally not deferring the speed change handling to `RateAdjustModHelper`
            // as the expected result of operation is not the same (daycore should preserve constant pitch).
            SpeedChange.BindValueChanged(val =>
            {
                freqAdjust.Value = SpeedChange.Default;
                tempoAdjust.Value = val.NewValue / SpeedChange.Default;
            }, true);
        }

        public override void ApplyToTrack(IAdjustableAudioComponent track)
        {
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
        }

        public override double ScoreMultiplier => rateAdjustHelper.ScoreMultiplier;
    }
}
