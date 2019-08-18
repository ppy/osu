// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTarget : Mod
    {
        public override string Name => "Target";
        public override string Acronym => "TP";
        public override ModType Type => ModType.Conversion;
        public override IconUsage Icon => OsuIcon.ModTarget;
        public override string Description => @"Practice keeping up with the beat of the song.";
        public override double ScoreMultiplier => 1;
    }
}
