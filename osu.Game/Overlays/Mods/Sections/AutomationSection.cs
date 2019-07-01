// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osuTK.Input;

namespace osu.Game.Overlays.Mods.Sections
{
    public class AutomationSection : ModSection
    {
        protected override Key[] ToggleKeys => new[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M };
        public override ModType ModType => ModType.Automation;

        public AutomationSection()
        {
            Header = @"Automation";
        }
    }
}
