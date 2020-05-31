// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Timing;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Denotes an object which provides a reference time to start animations from.
    /// </summary>
    /// <remarks>
    /// This should not be used to start an animation immediately at the current time.
    /// To do so, use <see cref="LegacySkinExtensions.GetAnimation"/> with <code>startAtCurrentTime = true</code> instead.
    /// </remarks>
    [Cached]
    public interface IAnimationTimeReference
    {
        /// <summary>
        /// The reference clock.
        /// </summary>
        IFrameBasedClock Clock { get; }

        /// <summary>
        /// The time which animations should be started from, relative to <see cref="Clock"/>.
        /// </summary>
        double AnimationStartTime { get; }
    }
}
