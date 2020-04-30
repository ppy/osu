// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRateAdjust : Mod, IApplicableToTrack
    {
        public abstract BindableNumber<double> SpeedChange { get; }

        public virtual void ApplyToTrack(Track track)
        {
            track.AddAdjustment(AdjustableProperty.Tempo, SpeedChange);
        }

        public override string SettingDescription => SpeedChange.IsDefault ? string.Empty : $"{SpeedChange.Value:N2}x";
    }
}
