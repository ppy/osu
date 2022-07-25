// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModPrecise : Mod
    {
        public override string Name => "Precise";

        public override string Acronym => "PR";

        public override string Description => "Very precise!";

        public override double ScoreMultiplier => 1.06;
    }
}