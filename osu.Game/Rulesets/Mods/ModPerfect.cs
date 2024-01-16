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
    public abstract class ModPerfect : ModFailCondition
    {
        public override string Name => "Perfect";
        public override string Acronym => "PF";
        public override IconUsage? Icon => OsuIcon.ModPerfect;
        public override ModType Type => ModType.DifficultyIncrease;
        public override double ScoreMultiplier => 1;
        public override LocalisableString Description => "SS or quit.";

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModSuddenDeath), typeof(ModAccuracyChallenge) }).ToArray();

        protected ModPerfect()
        {
            Restart.Value = Restart.Default = true;
        }

        protected override bool FailCondition(HealthProcessor healthProcessor, Judgement result)
            => (isRelevantResult(result.JudgementCriteria.MinResult) || isRelevantResult(result.JudgementCriteria.MaxResult) || isRelevantResult(result.Type))
               && result.Type != result.JudgementCriteria.MaxResult;

        private bool isRelevantResult(HitResult result) => result.AffectsAccuracy() || result.AffectsCombo();
    }
}
