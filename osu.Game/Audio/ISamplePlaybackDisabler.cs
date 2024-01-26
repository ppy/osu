// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Skinning;

namespace osu.Game.Audio
{
    /// <summary>
    /// Allows a component to disable sample playback dynamically as required.
    /// Automatically handled by <see cref="PausableSkinnableSamples"/>.
    /// May also be manually handled locally to particular components.
    /// </summary>
    [Cached]
    public interface ISamplePlaybackDisabler
    {
        /// <summary>
        /// Whether sample playback should be disabled (or paused for looping samples).
        /// </summary>
        IBindable<bool> SamplePlaybackDisabled { get; }
    }
}
