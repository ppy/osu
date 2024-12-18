// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics
{
    /// <summary>
    /// A simple container which blocks input events from travelling through it.
    /// </summary>
    public partial class InputBlockingContainer : Container
    {
        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnClick(ClickEvent e) => true;
    }
}
