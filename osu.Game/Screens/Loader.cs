// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Menu;
using OpenTK;

namespace osu.Game.Screens
{
    public class Loader : OsuScreen
    {
        public override bool ShowOverlays => false;

        public Loader()
        {
            ValidForResume = false;
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.RelativePositionAxes = Axes.Both;
            logo.Triangles = false;
            logo.Position = new Vector2(0.9f);
            logo.Scale = new Vector2(0.2f);

            logo.FadeInFromZero(5000, Easing.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            if (game.IsDeployedBuild)
                LoadComponentAsync(new Disclaimer(), d => Push(d));
            else
                LoadComponentAsync(new Intro(), d => Push(d));
        }
    }
}
