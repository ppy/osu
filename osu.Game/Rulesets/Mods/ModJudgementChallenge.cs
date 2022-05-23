// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModJudgementChallenge : ModChallenge
    {
        public override string Name => "Judgement Challenge";
        public override string Acronym => "JC";
        public override IconUsage? Icon => FontAwesome.Solid.Gavel;
        public override string Description => "Fail the beatmap if your judgements go above specified values.";

        [SettingSource("Maximum non-best judgements", "Maximum number of non-best judgements before fail (disregards judgement type).", 1)]
        public Bindable<int?> MaxNonBest { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        /// <summary>
        /// The maximum allowable number of each type <see cref="HitResult"/> for the challenge.
        /// Intended to be used with mod <see cref="SettingSourceAttribute"/> bindables.
        /// </summary>
        protected abstract IDictionary<HitResult, Bindable<int?>> HitResultMaximumCounts { get; }

        private readonly Dictionary<HitResult, int> hitResultCounts = new Dictionary<HitResult, int>();
        private int nonBestCount;

        protected ModJudgementChallenge()
        {
            foreach (HitResult key in Enum.GetValues(typeof(HitResult)).Cast<HitResult>())
                hitResultCounts.Add(key, 0);
        }

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            hitResultCounts[result.Type] += 1;
            nonBestCount += IsImperfectJudgement(result) ? 1 : 0;

            if (!AllowChallengeFailureAtHitObject(result.HitObject))
                return false;

            if (nonBestCount > MaxNonBest.Value)
                return true;

            foreach (var (key, value) in HitResultMaximumCounts)
                if (hitResultCounts[key] > value.Value)
                    return true;

            return false;
        }
    }
}