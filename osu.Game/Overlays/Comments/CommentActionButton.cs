// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Comments
{
    public class CommentActionButton : OsuHoverContainer
    {
        public CommentActionButton()
        {
            AutoSizeAxes = Axes.Both;
            IdleColour = OsuColour.Gray(0.7f);
            HoverColour = Color4.White;
        }
    }
}
