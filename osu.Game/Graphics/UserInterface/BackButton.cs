// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class BackButton : TwoLayerButton
    {
        public BackButton()
        {
            Text = @"back";
            Icon = FontAwesome.fa_osu_left_o;
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
