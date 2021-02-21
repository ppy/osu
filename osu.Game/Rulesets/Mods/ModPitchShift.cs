// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public class ModPitchShift : Mod, IApplicableToTrack
    {
        public override string Name => "Pitch Shift";
        public override string Acronym => "PS";
        public override IconUsage? Icon => FontAwesome.Solid.WaveSquare;
        public override ModType Type => ModType.Conversion;
        public override string Description => "Raise or lower the track's pitch.";
        public override double ScoreMultiplier => 1;

        [SettingSource("Pitch shift", "Raise or lower the track's pitch")]
        public BindableNumber<double> PitchChange { get; } = new BindableDouble()
        {
            MinValue = 0.5,
            MaxValue = 1.5,
            Default = 1,
            Value = 1,
            Precision = 0.01,
        };

        [SettingSource("Match tempo", "Match the pitch with the current tempo")]
        public BindableBool MatchTempo { get; } = new BindableBool();

        [SettingSource("Flatten pitch", "Counteract current pitch adjustments")]
        public BindableBool FlattenPitch { get; } = new BindableBool();

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        private ITrack track;

        public ModPitchShift()
        {
            PitchChange.BindValueChanged(val =>
            {
                tempoAdjust.Value = 1 / val.NewValue;
                freqAdjust.Value = val.NewValue;
            }, true);

            MatchTempo.BindValueChanged(applyMatchTempo);
            FlattenPitch.BindValueChanged(applyFlattenTempo);
        }

        public void ApplyToTrack(ITrack track)
        {
            this.track = track;

            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
        }

        private void applyMatchTempo(ValueChangedEvent<bool> val)
        {
            if (val.NewValue)
                PitchChange.Value = track.Tempo.Value;

            //PitchChange.Disabled = val.NewValue;
            //FlattenPitch.Disabled = val.NewValue;
        }

        private void applyFlattenTempo(ValueChangedEvent<bool> val)
        {
            if (val.NewValue)
                PitchChange.Value = 1.0 / track.Frequency.Value;

            //PitchChange.Disabled = val.NewValue;
            //MatchTempo.Disabled = val.NewValue;
        }
    }
}
