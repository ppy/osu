// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Audio
{
    public interface IReplayGainInfo : IEquatable<IReplayGainInfo>
    {
        public float PeakAmplitude { get; }
        public float TrackGain { get; }
    }
}
