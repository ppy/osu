// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public class DifficultyIncreaseSection : ModSection
    {
        protected override Key[] ToggleKeys => new[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L };
        public override ModType ModType => ModType.DifficultyIncrease;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.YellowLight;
        }

        public DifficultyIncreaseSection()
        {
            Header = @"Difficulty Increase";
        }
    }
}
