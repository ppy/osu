// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRateAdjust : Mod, IApplicableToRate
    {
        public sealed override bool ValidForMultiplayerAsFreeMod => false;

        public abstract BindableNumber<double> SpeedChange { get; }

        public abstract void ApplyToTrack(IAdjustableAudioComponent track);

        public virtual void ApplyToSample(IAdjustableAudioComponent sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public double ApplyToRate(double time, double rate) => rate * SpeedChange.Value;

        public override Type[] IncompatibleMods => new[] { typeof(ModTimeRamp), typeof(ModAdaptiveSpeed), typeof(ModRateAdjust) };

        public override string SettingDescription => SpeedChange.IsDefault ? string.Empty : $"{SpeedChange.Value:N2}x";

        public override string ExtendedIconInformation => SettingDescription;

        public override bool Ranked =>
            // only allow speed change to be ranked to be sure that any mods that inherit this one don't accidentally make new settings ranked too
            SettingsBindables.Where(s => s != SpeedChange).All(s => s.IsDefault);
    }
}
