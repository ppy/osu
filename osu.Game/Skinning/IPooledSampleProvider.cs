// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Provides pooled samples to be used by <see cref="SkinnableSound"/>s.
    /// </summary>
    public interface IPooledSampleProvider
    {
        /// <summary>
        /// Retrieves a <see cref="PoolableSkinnableSample"/> from a pool.
        /// </summary>
        /// <param name="sampleInfo">The <see cref="SampleInfo"/> describing the sample to retrieve.</param>
        /// <returns>The <see cref="PoolableSkinnableSample"/>.</returns>
        PoolableSkinnableSample? GetPooledSample(ISampleInfo sampleInfo);
    }
}
