// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSpunOut : Mod
    {
        public override string Name => "Spun Out";
        public override string Acronym => "SO";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_spunout;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => @"Spinners will be automatically completed.";
        public override double ScoreMultiplier => 0.9;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(OsuModAutopilot) };
    }
}
