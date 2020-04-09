// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRandom : Mod
    {
        public override string Name => "Random";
        public override string Acronym => "RD";
        public override ModType Type => ModType.Conversion;
        public override IconUsage? Icon => OsuIcon.Dice;
        public override double ScoreMultiplier => 1;
    }
}
