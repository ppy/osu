// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRateAdjust : Mod, IApplicableToAudio
    {
        public abstract BindableNumber<double> SpeedChange { get; }

        public virtual void ApplyToTrack(ITrack track)
        {
            (track as IAdjustableAudioComponent)?.AddAdjustment(AdjustableProperty.Tempo, SpeedChange);
        }

        public virtual void ApplyToSample(SampleChannel sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public override string SettingDescription => SpeedChange.IsDefault ? string.Empty : $"{SpeedChange.Value:N2}x";
    }
}
