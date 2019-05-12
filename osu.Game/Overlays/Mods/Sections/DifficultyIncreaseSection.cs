// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osuTK.Input;

namespace osu.Game.Overlays.Mods.Sections
{
    public class DifficultyIncreaseSection : ModSection
    {
        protected override Key[] ToggleKeys => new[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L };
        public override ModType ModType => ModType.DifficultyIncrease;

        public DifficultyIncreaseSection()
        {
            Header = @"Difficulty Increase";
        }
    }
}
