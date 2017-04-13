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
    /// Tooltip with custom appear time
    /// </summary>
    public interface IHasDelayedTooltip : IHasTooltip
    {
        /// <summary>
        /// Time until the tooltip appears (in milliseconds)
        /// </summary>
        int Delay { get; }
    }

    /// <summary>
    /// Tooltip which can show after hovering over the object
    /// </summary>
    public interface IHasOverhangingTooltip : IHasTooltip
    {
        /// <summary>
        /// Should the tooltip still show?
        /// </summary>
        bool Overhanging { get; }
    }
}
