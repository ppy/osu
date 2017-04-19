// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Graphics.Cursor
{
    public interface IHasTooltip : IDrawable
    {
        /// <summary>
        /// Tooltip that shows when hovering the drawable
        /// </summary>
        string TooltipText { get; }
    }

    /// <summary>
    /// Tooltip with custom appear time
    /// </summary>
    public interface IHasTooltipWithCustomDelay : IHasTooltip
    {
        /// <summary>
        /// Time until the tooltip appears (in milliseconds)
        /// </summary>
        int TooltipDelay { get; }
    }

    /// <summary>
    /// Tooltip which can decide when to disappear
    /// </summary>
    public interface IHasDisappearingTooltip : IHasTooltip
    {
        /// <summary>
        /// Should the tooltip disappear?
        /// </summary>
        bool Disappear { get; }
    }
}
