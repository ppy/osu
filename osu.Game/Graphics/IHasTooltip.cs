// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Graphics
{
    public interface IHasTooltip : IDrawable
    {
        /// <summary>
        /// Tooltip that shows when hovering the drawable
        /// </summary>
        string TooltipText { get; }
    }
}
