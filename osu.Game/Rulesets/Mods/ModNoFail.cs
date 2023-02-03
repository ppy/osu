// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation.Mods;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNoFail : ModBlockFail
    {
        public override string Name => "No Fail";
        public override string Acronym => "NF";
        public override IconUsage? Icon => OsuIcon.ModNoFail;
        public override ModType Type => ModType.DifficultyReduction;
        public override LocalisableString Description => DifficultyReductionStrings.NoFailDescription;
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax), typeof(ModFailCondition), typeof(ModAutoplay) };
    }
}
