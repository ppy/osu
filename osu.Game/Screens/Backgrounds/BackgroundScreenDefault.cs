﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenDefault : BackgroundScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new Background(@"Backgrounds/bg1"));
        }
    }
}