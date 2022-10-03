// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public class ModPitchShift : Mod, IApplicableToRate
    {
        public override string Name => "Pitch Shift";
        public override string Acronym => "PS";
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Adjust the pitch of the song!";
        public override double ScoreMultiplier => 1;

        public override bool RequiresConfiguration => true;

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        // 12th root of 2. multiplying any frequency by this constant will increase it by one semitone
        private const double semitone_multiplier = 1.059463;

        [SettingSource("Pitch change", "The change to the pitch, in semitones")]
        public BindableNumber<int> PitchChange { get; } = new BindableInt
        {
            MinValue = -12,
            MaxValue = 12,
        };

        public ModPitchShift()
        {
            PitchChange.BindValueChanged(val =>
            {
                freqAdjust.Value = Math.Pow(semitone_multiplier, val.NewValue);
                tempoAdjust.Value = 1 / freqAdjust.Value;
            });
        }

        public virtual void ApplyToTrack(IAdjustableAudioComponent track)
        {
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
        }

        public virtual void ApplyToSample(IAdjustableAudioComponent sample) { }
        public double ApplyToRate(double time, double rate) => rate;

        public override string SettingDescription => PitchChange.Value.ToString("+0;-#");
        public override Type[] IncompatibleMods => new[] { typeof(ModDaycore), typeof(ModNightcore), typeof(ModTimeRamp) };
    }
}
