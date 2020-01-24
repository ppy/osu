// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTouchDevice : Mod
    {
        public override string Name => "Touch Device";
        public override string Acronym => "TD";
        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.System;

        public override bool Ranked => true;
    }
}
