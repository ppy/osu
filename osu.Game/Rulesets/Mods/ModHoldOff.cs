// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHoldOff : Mod
    {
        public override string Name => "Hold Off";

        public override string Acronym => "HO";

        public override IconUsage? Icon => FontAwesome.Solid.DotCircle;

        public override ModType Type => ModType.Conversion;
    }
}
