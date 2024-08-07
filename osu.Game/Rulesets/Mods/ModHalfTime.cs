// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHalfTime : ModRateAdjust
    {
        public override string Name => "Half Time";
        public override string Acronym => "HT";
        public override IconUsage? Icon => OsuIcon.ModHalftime;
        public override ModType Type => ModType.DifficultyReduction;
        public override LocalisableString Description => "Less zoom...";

        [SettingSource("Speed decrease", "The actual decrease to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble(0.75)
        {
            MinValue = 0.5,
            MaxValue = 0.99,
            Precision = 0.01,
        };

        [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
        public virtual BindableBool AdjustPitch { get; } = new BindableBool();

        private readonly RateAdjustModHelper rateAdjustHelper;

        protected ModHalfTime()
        {
            rateAdjustHelper = new RateAdjustModHelper(SpeedChange);
            rateAdjustHelper.HandleAudioAdjustments(AdjustPitch);
        }

        public override void ApplyToTrack(IAdjustableAudioComponent track)
        {
            rateAdjustHelper.ApplyToTrack(track);
        }

        public override double ScoreMultiplier => rateAdjustHelper.ScoreMultiplier;
    }
}
