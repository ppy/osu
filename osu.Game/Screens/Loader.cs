// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Menu;
using OpenTK;
using osu.Framework.Screens;

namespace osu.Game.Screens
{
    public class Loader : OsuScreen
    {
        private bool showDisclaimer;

        public override bool ShowOverlays => false;

        public Loader()
        {
            ValidForResume = false;
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.Triangles = false;
            logo.Origin = Anchor.BottomRight;
            logo.Anchor = Anchor.BottomRight;
            logo.Position = new Vector2(-40);
            logo.Scale = new Vector2(0.2f);

            logo.FadeInFromZero(5000, Easing.OutQuint);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            if (showDisclaimer)
                LoadComponentAsync(new Disclaimer(), d => Push(d));
            else
                LoadComponentAsync(new Intro(), d => Push(d));
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            base.LogoSuspending(logo);
            logo.FadeOut(100);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            showDisclaimer = game.IsDeployedBuild;
        }
    }
}
