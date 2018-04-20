// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using System;

namespace osu.Game.Screens.Menu
{
    public class ExitProgressOverlay : OverlayContainer
    {
        public override bool HandleKeyboardInput => false;
        private readonly Box box;

        public ExitProgressOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                box = new Box
                {
                    Colour = OsuColour.FromHex(@"000000"),
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        public void SetProgress(double progress)
        {
            box.Alpha = Math.Min((float)progress, 1f);
        }

        protected override void PopIn() => Alpha = 1;
        protected override void PopOut() => Alpha = 0;
    }
}
