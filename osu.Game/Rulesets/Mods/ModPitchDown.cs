// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public class ModPitchDown : Mod, IApplicableToTrack
    {
        public override string Name => "Pitch Down";
        public override string Acronym => "PD";
        public override IconUsage? Icon => OsuIcon.ModDaycore; // Temporary Icon
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Decrease pitch without affecting playback speed.";
        public override double ScoreMultiplier => 1;
        public override bool ValidForFreestyleAsRequiredMod => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModRateAdjust), typeof(ModAdaptiveSpeed), typeof(ModTimeRamp), typeof(ModPitchUp) };

        [SettingSource("Pitch multiplier", "Adjusts pitch while speed remains constant.")]
        public BindableNumber<double> PitchMultiplier { get; } = new BindableDouble(0.75)
        {
            MinValue = 0.50,
            MaxValue = 0.99,
            Precision = 0.01,
        };

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        public ModPitchDown()
        {
            PitchMultiplier.BindValueChanged(val =>
            {
                freqAdjust.Value = val.NewValue;
                tempoAdjust.Value = 1 / val.NewValue;
            }, true);
        }

        public void ApplyToTrack(IAdjustableAudioComponent track)
        {
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
        }
    }
}
