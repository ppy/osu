// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRateChanger : ModRateAdjust
    {
        public override string Name => "Rate Changer";
        public override string Acronym => "RC";
        public override IconUsage? Icon => OsuIcon.ModHalftime;
        public override ModType Type => ModType.Conversion;
        public override LocalisableString Description => "Change speed and pitch separately for any song";

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);

        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        public double speedMultiplier = 1;

        bool allowLog = true;
        
        [SettingSource("Speed edit", "The song speed multiplier")]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble(1)
        {
            MinValue = 0.1,
            MaxValue = 2,
            Precision = 0.01,
        };

        [SettingSource("Pitch edit", "The song pitch multiplier")]
        public BindableNumber<double> PitchChange { get; } = new BindableDouble(1)
        {
            MinValue = 0.1,
            MaxValue = 2,
            Precision = 0.01,
        };

        protected ModRateChanger()
        {
            SpeedChange.BindValueChanged(val =>
            {
                speedMultiplier = val.NewValue;

                ValTempoReal();
            }, true);

            PitchChange.BindValueChanged(val =>
            {
                freqAdjust.Value = val.NewValue;

                ValTempoReal();
            }, true);
        }

        public override void ApplyToTrack(IAdjustableAudioComponent track)
        {
            // base.ApplyToTrack() intentionally not called (different tempo adjustment is applied)
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
        }
        
        // Implements 1/x to define the value of tempoAdjust.
        void ValTempoReal() {
            if ((speedMultiplier * (1/freqAdjust.Value)) > 0.05)
            {
                tempoAdjust.Value = speedMultiplier * (1/freqAdjust.Value);
                allowLog = true;
            }
            else if (allowLog)
            {
                Logger.Log("Resulting internal tempo would be smaller than 0,05.", LoggingTarget.Information, LogLevel.Important);
                allowLog = false;
            }
        }
    }
}
