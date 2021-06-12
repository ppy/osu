// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModClassic : Mod
    {
        public override string Name => "经典";

        public override string Acronym => "CL";

        public override double ScoreMultiplier => 1;

        public override IconUsage? Icon => FontAwesome.Solid.History;

        public override string Description => "梦回V1";

        public override ModType Type => ModType.Conversion;
    }
}
