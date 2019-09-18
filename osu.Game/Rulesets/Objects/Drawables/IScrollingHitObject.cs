// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Objects.Drawables
{
    /// <summary>
    /// An interface that exposes properties required for scrolling hit objects to be properly displayed.
    /// </summary>
    internal interface IScrollingHitObject : IDrawable
    {
        /// <summary>
        /// Time offset before the hit object start time at which this <see cref="IScrollingHitObject"/> becomes visible and the time offset
        /// after the hit object's end time after which it expires.
        ///
        /// <para>
        /// This provides only a default life time range, however classes inheriting from <see cref="IScrollingHitObject"/> should override
        /// their life times if more tight control is desired.
        /// </para>
        /// </summary>
        BindableDouble LifetimeOffset { get; }

        /// <summary>
        /// Axes which this <see cref="IScrollingHitObject"/> will scroll through.
        /// This is set by the container which this scrolls through.
        /// </summary>
        Axes ScrollingAxes { set; }
    }
}
