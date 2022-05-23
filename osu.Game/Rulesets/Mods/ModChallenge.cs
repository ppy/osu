// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModChallenge : ModFailCondition, IApplicableToBeatmap
    {
        public override ModType Type => ModType.DifficultyIncrease;
        public override double ScoreMultiplier => 1;
        public override bool RequiresConfiguration => true;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModPerfect)).Append(typeof(ModEasyWithExtraLives)).ToArray();

        [SettingSource("Checking interval", "When conditions will be checked.")]
        public Bindable<ChallengeCheckInterval> CheckingInterval { get; } = new Bindable<ChallengeCheckInterval>();

        protected readonly List<HitObject> HitObjectsBeforeBreaks = new List<HitObject>();
        protected HitObject LastHitObjectInBeatmap;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            switch (CheckingInterval.Value)
            {
                case ChallengeCheckInterval.AtEnd:
                    LastHitObjectInBeatmap = beatmap.HitObjects.LastOrDefault();
                    break;

                case ChallengeCheckInterval.AtBreak:
                    for (int i = 0; i < beatmap.HitObjects.Count - 1; i++)
                    {
                        double inBetweenTime = (beatmap.HitObjects.ElementAtOrDefault(i).GetEndTime() + beatmap.HitObjects.ElementAtOrDefault(i + 1).StartTime) / 2;
                        foreach (BreakPeriod breakPeriod in beatmap.Breaks)
                        {
                            if (breakPeriod.Contains(inBetweenTime))
                                HitObjectsBeforeBreaks.Add(beatmap.HitObjects.ElementAtOrDefault(i));
                        }
                    }
                    break;
            }
        }

        public virtual ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        /// <summary>
        /// Check whether the <see cref="CheckingInterval"/> allows for a challenge-induced failure to take place at this <see cref="HitObject"/>.
        /// </summary>
        /// <returns>Whether a challenge-induced failure is allowed to take place.</returns>
        protected bool AllowChallengeFailureAtHitObject(HitObject hitObject)
        {
            switch (CheckingInterval.Value)
            {
                case ChallengeCheckInterval.AtBreak:
                    if (!HitObjectsBeforeBreaks.Contains(hitObject)) return false;
                    break;

                case ChallengeCheckInterval.AtEnd:
                    if (hitObject != LastHitObjectInBeatmap)
                        return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the <see cref="JudgementResult"/> is anything less than the best it can possibly be.
        /// </summary>
        /// <param name="judgement">The judgement to check.</param>
        /// <returns>Whether the judgement is the best it can possibly be.</returns>
        protected bool JudgementIsFlawed(JudgementResult judgement)
        {
            return !(judgement.Type == judgement.Judgement.MaxResult || judgement.Type == HitResult.LargeTickHit) && judgement.Type.AffectsCombo();
        }

        public enum ChallengeCheckInterval
        {
            Continuously,
            AtBreak,
            AtEnd, // doesn't make sense to be compatible with `ModEasyWithExtraLives` if you're checking the last hitobject
        }
    }
}
