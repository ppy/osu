// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public interface IScrollingInfo
    {
        /// <summary>
        /// The direction <see cref="HitObject"/>s should scroll in.
        /// </summary>
        IBindable<ScrollingDirection> Direction { get; }

        /// <summary>
        /// The span of time that is visible by the length of the scrolling axes.
        /// </summary>
        IBindable<double> TimeRange { get; }

        /// <summary>
        /// The algorithm which controls <see cref="HitObject"/> positions and sizes.
        /// </summary>
        IBindable<IScrollAlgorithm> Algorithm { get; }
    }
}
