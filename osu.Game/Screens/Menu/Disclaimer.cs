// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Screens;

namespace osu.Game.Screens.Menu
{
    class Disclaimer : OsuScreen
    {
        private Intro intro;
        internal override bool ShowOverlays => false;

        [BackgroundDependencyLoader]
        private void load(OsuGame game)
        {
            (intro = new Intro()).Preload(game);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            FadeInFromZero(100);

            Delay(5000);

            FadeOut(100);

            Push(intro);
        }
    }
}
