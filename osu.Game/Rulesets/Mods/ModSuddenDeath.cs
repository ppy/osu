// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModSuddenDeath : ModFailCondition
    {
        public override string Name => "Sudden Death";
        public override string Acronym => "SD";
        public override IconUsage? Icon => OsuIcon.ModSuddenDeath;
        public override ModType Type => ModType.DifficultyIncrease;
        public override LocalisableString Description => "Miss and fail.";
        public override double ScoreMultiplier => 1;
        public override bool EligibleForPP => true;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModPerfect)).ToArray();

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
            => result.Type.AffectsCombo()
               && !result.IsHit;
    }
}
