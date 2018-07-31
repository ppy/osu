// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;
using OpenTK.Input;

namespace osu.Game.Overlays.Mods.Sections
{
    public class ConversionSection : ModSection
    {
        protected override Key[] ToggleKeys => null;
        public override ModType ModType => ModType.Conversion;

        public ConversionSection()
        {
            Header = @"Conversion";
        }
    }
}
