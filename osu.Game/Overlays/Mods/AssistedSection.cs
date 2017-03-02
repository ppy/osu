// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes;
using osu.Game.Overlays.Mods;

namespace osu.Game
{
    public class AssistedSection : ModSection
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Blue;
            SelectedColour = colours.BlueLight;
        }

        public AssistedSection()
        {
            Header = @"Assisted";
        }
    }
}
