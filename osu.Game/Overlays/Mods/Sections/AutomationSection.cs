// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;
using OpenTK.Input;

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
