// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class BackButton : TwoLayerButton
    {
        public BackButton()
        {
            Text = @"back";
            Icon = OsuIcon.LeftCircle;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.Pink;
            HoverColour = colours.PinkDark;
        }
    }
}
