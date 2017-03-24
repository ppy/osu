// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A type of circle piece that will be scrolling the screen.
    /// <para>
    /// A scrolling circle piece must always have a centre-left origin due to how scroll position is calculated.
    /// </para>
    /// </summary>
    public class ScrollingPiece : Container
    {
        public override Anchor Origin => Anchor.CentreLeft;
    }
}