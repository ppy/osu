// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Allows a component to disable sample playback dynamically as required.
    /// Handled by <see cref="PausableSkinnableSound"/>.
    /// </summary>
    public interface ISamplePlaybackDisabler
    {
        /// <summary>
        /// Whether sample playback should be disabled (or paused for looping samples).
        /// </summary>
        IBindable<bool> SamplePlaybackDisabled { get; }
    }
}
