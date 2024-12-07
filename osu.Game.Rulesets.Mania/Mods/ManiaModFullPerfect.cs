﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.4

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFullPerfect : ManiaModPerfect
    {
        public override string Name => "Full Perfect";
        public override string Acronym => "FP";
        public override LocalisableString Description => @"Placeholder";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
            {
            typeof(ManiaModPerfect),
        }).ToArray();


        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            if (!isRelevantResult(result.Judgement.MinResult) && !isRelevantResult(result.Judgement.MaxResult) && !isRelevantResult(result.Type))
                return false;

            if (result.Judgement.MaxResult == HitResult.Perfect)
                return result.Type < HitResult.Perfect;

            return result.Type != result.Judgement.MaxResult;
        }

        private bool isRelevantResult(HitResult result) => result.AffectsAccuracy() || result.AffectsCombo();
    }
}
