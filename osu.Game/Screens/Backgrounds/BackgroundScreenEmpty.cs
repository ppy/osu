// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK.Graphics;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenBlack : BackgroundScreen
    {
        public BackgroundScreenBlack()
        {
            Child = new Box
            {
                Colour = Color4.Black,
                RelativeSizeAxes = Axes.Both,
            };
        }

        protected override void OnEntering(Screen last)
        {
            Show();
        }
    }
}
