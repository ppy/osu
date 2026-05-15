// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK.Graphics;

namespace osu.Game.Screens.Backgrounds
{
    public partial class BackgroundScreenBlack : BackgroundScreen
    {
        private readonly double delayBeforeBlack;
        private readonly Box box;

        public BackgroundScreenBlack(double delayBeforeBlack = 0)
        {
            this.delayBeforeBlack = delayBeforeBlack;

            InternalChild = box = new Box
            {
                Colour = Color4.Black,
                RelativeSizeAxes = Axes.Both,
            };

            Alpha = 0;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            this
                .Delay(delayBeforeBlack)
                .FadeIn(200)
                .OnComplete(_ => box.Hide());
        }
    }
}
