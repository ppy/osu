// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Graphics.Cursor
{
    public interface IHasTooltip
    {
        /// <summary>
        /// Tooltip that shows when hovering the object
        /// </summary>
        string Tooltip { get; }
    }

    /// <summary>
    /// Interface of <see cref="IHasTooltip"/> with custom appear time
    /// </summary>
    public interface IHasDelayedTooltip : IHasTooltip
    {
        /// <summary>
        /// Time until the tooltip appears (in milliseconds)
        /// </summary>
        int Delay { get; }
    }
}
