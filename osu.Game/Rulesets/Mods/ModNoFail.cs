// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNoFail : Mod, IApplicableFailOverride
    {
        public override string Name => "No Fail";
        public override string Acronym => "NF";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "You can't fail, no matter what.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModAutoplay) };

        /// <summary>
        /// We never fail, 'yo.
        /// </summary>
        public bool AllowFail => false;
    }
}
