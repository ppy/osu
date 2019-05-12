// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osuTK.Input;

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
