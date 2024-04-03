// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Audio
{
    public interface IAudioNormalization : IEquatable<IAudioNormalization>
    {
        public float VolumeOffset { get; }
        public float IntegratedLoudness { get; }
    }
}
