// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Track;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModTimeAdjust : Mod, IApplicableToTrack
    {
        public override Type[] IncompatibleMods => new[] { typeof(ModTimeRamp) };

        protected abstract double RateAdjust { get; }

        public virtual void ApplyToTrack(Track track)
        {
            track.Tempo.Value *= RateAdjust;
        }
    }
}
