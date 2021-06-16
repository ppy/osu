// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Provides pooled samples to be used by <see cref="SkinnableSound"/>s.
    /// </summary>
    internal interface IPooledSampleProvider
    {
        /// <summary>
        /// Retrieves a <see cref="PoolableSkinnableSample"/> from a pool.
        /// </summary>
        /// <param name="sampleInfo">The <see cref="SampleInfo"/> describing the sample to retrieve.</param>
        /// <returns>The <see cref="PoolableSkinnableSample"/>.</returns>
        [CanBeNull]
        PoolableSkinnableSample GetPooledSample(ISampleInfo sampleInfo);
    }
}
