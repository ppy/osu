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
    public class ModPitchAdjust : Mod, IApplicableToTrack
    {
        public override string Name => "Pitch Adjust";
        public override string Acronym => "PA";
        public override IconUsage? Icon => OsuIcon.ModNightcore; // Temporary Icon
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Adjust pitch without affecting playback speed.";
        public override double ScoreMultiplier => 1;
        public override bool ValidForFreestyleAsRequiredMod => true;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModRateAdjust), typeof(ModAdaptiveSpeed), typeof(ModTimeRamp) };

        [SettingSource("Semitones", "Adjusts pitch in semitone steps.")]
        public BindableNumber<int> Semitones { get; } = new BindableInt(0)
        {
            MinValue = -12,
            MaxValue = 12,
            Precision = 1,
        };

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        public ModPitchAdjust()
        {
            Semitones.BindValueChanged(val =>
            {
                double multiplier = Math.Pow(2, val.NewValue / 12.0);

                freqAdjust.Value = multiplier;
                tempoAdjust.Value = 1 / multiplier;
            }, true);
        }

        public void ApplyToTrack(IAdjustableAudioComponent track)
        {
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
        }
    }
}
