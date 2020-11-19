// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    public interface IPooledSampleProvider
    {
        [CanBeNull]
        PoolableSkinnableSample GetPooledSample(ISampleInfo sampleInfo);
    }
}
