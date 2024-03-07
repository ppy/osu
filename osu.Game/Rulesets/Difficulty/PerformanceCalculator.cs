// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public Task<PerformanceAttributes> CalculateAsync(ScoreInfo score, DifficultyAttributes attributes, CancellationToken cancellationToken)
            => Task.Run(() => CreatePerformanceAttributes(score, attributes), cancellationToken);

        public PerformanceAttributes Calculate(ScoreInfo score, DifficultyAttributes attributes)
            => CreatePerformanceAttributes(score, attributes);

        public PerformanceAttributes Calculate(ScoreInfo score, IWorkingBeatmap beatmap)
            => Calculate(score, Ruleset.CreateDifficultyCalculator(beatmap).Calculate(score.Mods));

        /// <summary>
        /// Creates <see cref="PerformanceAttributes"/> to describe a score's performance.
        /// </summary>
        /// <param name="score">The score to create the attributes for.</param>
        /// <param name="attributes">The difficulty attributes for the beatmap relating to the score.</param>
        protected abstract PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes);

        public Task<PerformanceAttributes> GetPerfectPerformanceAsync(IWorkingBeatmap beatmap, Mod[]? mods = null, CancellationToken cancellationToken = default)
            => Task.Run(() => GetPerfectPerformance(beatmap, mods), cancellationToken);

        public Task<PerformanceAttributes> GetPerfectPerformanceAsync(IBeatmap playableBeatmap, DifficultyAttributes attributes, Mod[]? mods = null, CancellationToken cancellationToken = default)
            => Task.Run(() => GetPerfectPerformance(playableBeatmap, attributes, mods), cancellationToken);

        /// <summary>
        /// Calculate <see cref="PerformanceAttributes"/> to describe a perfect play performance.
        /// </summary>
        /// <param name="beatmap">The beatmap used for calculations.</param>
        /// <param name="mods">Mods required for calculation.</param>
        public PerformanceAttributes GetPerfectPerformance(IWorkingBeatmap beatmap, Mod[]? mods = null)
            => GetPerfectPerformance(beatmap.GetPlayableBeatmap(Ruleset.RulesetInfo, mods ?? Array.Empty<Mod>()),
                Ruleset.CreateDifficultyCalculator(beatmap).Calculate(mods ?? Array.Empty<Mod>()), mods);

        /// <summary>
        /// Calculate <see cref="PerformanceAttributes"/> to describe a perfect play performance.
        /// </summary>
        /// <param name="playableBeatmap">The beatmap used for calculations.</param>
        /// <param name="attributes">The difficulty attributes for the beatmap relating to the score.</param>
        /// <param name="mods">Mods required for calculation.</param>
        public PerformanceAttributes GetPerfectPerformance(IBeatmap playableBeatmap, DifficultyAttributes attributes, Mod[]? mods = null)
        {
            ScoreInfo perfectPlay = new ScoreInfo(playableBeatmap.BeatmapInfo, Ruleset.RulesetInfo)
            {
                Accuracy = 1,
                Passed = true
            };
            if (mods != null) perfectPlay.Mods = mods;

            // calculate max combo
            // todo: Get max combo from difficulty calculator instead when diffcalc properly supports lazer-first scores
            perfectPlay.MaxCombo = calculateMaxCombo(playableBeatmap);

            // create statistics assuming all hit objects have perfect hit result
            var statistics = playableBeatmap.HitObjects
                                            .SelectMany(getPerfectHitResults)
                                            .GroupBy(hr => hr, (hr, list) => (hitResult: hr, count: list.Count()))
                                            .ToDictionary(pair => pair.hitResult, pair => pair.count);
            perfectPlay.Statistics = statistics;
            perfectPlay.MaximumStatistics = statistics;

            // calculate total score
            ScoreProcessor scoreProcessor = Ruleset.CreateScoreProcessor();
            scoreProcessor.ApplyBeatmap(playableBeatmap);
            perfectPlay.TotalScore = scoreProcessor.MaximumTotalScore;

            // compute rank achieved
            // default to SS, then adjust the rank with mods
            perfectPlay.Rank = ScoreRank.X;

            foreach (IApplicableToScoreProcessor mod in perfectPlay.Mods.OfType<IApplicableToScoreProcessor>())
            {
                perfectPlay.Rank = mod.AdjustRank(perfectPlay.Rank, 1);
            }

            // calculate performance for this perfect score.
            return CreatePerformanceAttributes(perfectPlay, attributes);
        }

        private int calculateMaxCombo(IBeatmap beatmap)
        {
            return beatmap.HitObjects.SelectMany(getPerfectHitResults).Count(r => r.AffectsCombo());
        }

        private IEnumerable<HitResult> getPerfectHitResults(HitObject hitObject)
        {
            foreach (HitObject nested in hitObject.NestedHitObjects)
                yield return nested.Judgement.MaxResult;

            yield return hitObject.Judgement.MaxResult;
        }
    }
}
