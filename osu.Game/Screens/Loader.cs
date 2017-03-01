﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;

namespace osu.Game.Screens
{
    class Loader : OsuScreen
    {
        internal override bool ShowOverlays => false;

        public Loader()
        {
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame game)
        {
            if (game.IsDeployedBuild)
                new Disclaimer().LoadAsync(game, d => Push((Screen)d));
            else
                new Intro().LoadAsync(game, d => Push((Screen)d));
        }
    }
}
