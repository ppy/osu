// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public class AssistedSection : ModSection
    {
        protected override Key[] ToggleKeys => new[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M };
        public override ModType ModType => ModType.Special;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ButtonColour = colours.Blue;
            SelectedColour = colours.BlueLight;
        }

        public AssistedSection()
        {
            Header = @"Assisted";
        }
    }
}
