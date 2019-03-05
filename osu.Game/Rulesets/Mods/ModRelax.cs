// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRelax : Mod
    {
        public override string Name => "Relax";
        public override string Acronym => "RX";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_relax;
        public override ModType Type => ModType.Automation;
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModNoFail), typeof(ModSuddenDeath) };
    }
}
