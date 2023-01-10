// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class PerformanceCalculator
    {
        protected readonly Ruleset Ruleset;

        protected PerformanceCalculator(Ruleset ruleset)
        {
            Ruleset = ruleset;
        }

        public PerformanceAttributes Calculate(ScoreInfo score, DifficultyAttributes attributes)
            => CreatePerformanceAttributes(score, attributes);

        public PerformanceAttributes Calculate(ScoreInfo score, IWorkingBeatmap beatmap)
            => Calculate(score, Ruleset.CreateDifficultyCalculator(beatmap).Calculate(score.Mods));

        public PerformanceAttributes CalculatePerfectPerformance(ScoreInfo score, IWorkingBeatmap beatmap)
        {
            ScoreInfo perfectPlay = score.DeepClone();
            IBeatmap playableBeatmap = beatmap.GetPlayableBeatmap(Ruleset.RulesetInfo);
            perfectPlay.Accuracy = 1;
            perfectPlay.Passed = true;
            perfectPlay.MaxCombo = playableBeatmap.GetMaxCombo();

            // create statistics assuming all hit objects have perfect hit result
            var statistics = playableBeatmap.HitObjects
                                            .SelectMany(getPerfectHitResults)
                                            .GroupBy(hr => hr, (hr, list) => (hitResult: hr, count: list.Count()))
                                            .ToDictionary(pair => pair.hitResult, pair => pair.count);
            perfectPlay.Statistics = statistics;

            // calculate total score
            ScoreProcessor scoreProcessor = Ruleset.CreateScoreProcessor();
            scoreProcessor.Mods.Value = perfectPlay.Mods;
            perfectPlay.TotalScore = scoreProcessor.ComputeScore(ScoringMode.Standardised, perfectPlay);

            // compute rank achieved
            // default to SS, then adjust the rank with mods
            perfectPlay.Rank = ScoreRank.X;

            foreach (IApplicableToScoreProcessor mod in perfectPlay.Mods.OfType<IApplicableToScoreProcessor>())
            {
                perfectPlay.Rank = mod.AdjustRank(perfectPlay.Rank, 1);
            }

            DifficultyAttributes difficulty = Ruleset.CreateDifficultyCalculator(beatmap).Calculate(score.Mods);

            return Calculate(perfectPlay, difficulty);
        }

        private IEnumerable<HitResult> getPerfectHitResults(HitObject hitObject)
        {
            foreach (HitObject nested in hitObject.NestedHitObjects)
                yield return nested.CreateJudgement().MaxResult;

            yield return hitObject.CreateJudgement().MaxResult;
        }

        /// <summary>
        /// Creates <see cref="PerformanceAttributes"/> to describe a score's performance.
        /// </summary>
        /// <param name="score">The score to create the attributes for.</param>
        /// <param name="attributes">The difficulty attributes for the beatmap relating to the score.</param>
        protected abstract PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes);
    }
}
