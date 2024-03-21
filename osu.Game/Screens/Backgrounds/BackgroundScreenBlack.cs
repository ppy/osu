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
        public BackgroundScreenBlack()
        {
            InternalChild = new Box
            {
                Colour = Color4.Black,
                RelativeSizeAxes = Axes.Both,
            };
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            Show();
        }
    }
}
