// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A type of container which spans a length of time.
    /// </summary>
    public interface IHasTimeSpan : IContainer
    {
        /// <summary>
        /// The amount of time which this container spans.
        /// </summary>
        double TimeSpan { get; }
    }
}