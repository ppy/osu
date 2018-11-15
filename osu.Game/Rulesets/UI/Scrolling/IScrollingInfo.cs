// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
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
        ///
        /// </summary>
        IBindable<double> TimeRange { get; }

        /// <summary>
        /// The algorithm which controls <see cref="HitObject"/> positions and sizes.
        /// </summary>
        IScrollAlgorithm Algorithm { get; }
    }
}
