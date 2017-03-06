// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Mods
{
    public class AssistedSection : ModSection
    {
        protected override Key[] ToggleKeys => new Key[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M };

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
