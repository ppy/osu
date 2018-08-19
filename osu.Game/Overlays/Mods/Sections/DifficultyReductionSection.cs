// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;
using OpenTK.Input;

namespace osu.Game.Overlays.Mods.Sections
{
    public class DifficultyReductionSection : ModSection
    {
        protected override Key[] ToggleKeys => new[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P };
        public override ModType ModType => ModType.DifficultyReduction;

        public DifficultyReductionSection()
        {
            Header = @"Difficulty Reduction";
        }
    }
}
