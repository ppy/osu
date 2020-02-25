// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Comments
{
    public class CommentActionButton : OsuHoverContainer
    {
        public CommentActionButton()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            IdleColour = colourProvider.Foreground1;
            HoverColour = Color4.White;
        }
    }
}
