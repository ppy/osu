// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Utils
{
    public static class ScoreUtils
    {
        /// <summary>
        /// Simulates perfect play on <see cref="Beatmap"/>.
        /// </summary>
        /// <param name="beatmap">Beatmap, on which perfect play will be simulated</param>
        /// <param name="ruleset">Ruleset, for which perfect play will be simulated</param>
        /// <param name="mods">(Optional) set of mods that will be accounted for in simulation</param>
        /// <param name="baseScore">(Optional) score that will be used as a base</param>
        /// <returns><see cref="ScoreInfo"/> of simulated play</returns>
        public static ScoreInfo GetPerfectPlay(IBeatmap beatmap, RulesetInfo ruleset, Mod[]? mods = null, ScoreInfo? baseScore = null)
        {
            Ruleset rulesetInstance = ruleset.CreateInstance();
            ScoreInfo perfectPlay = baseScore == null ? new ScoreInfo(beatmap.BeatmapInfo, ruleset) : baseScore.DeepClone();

            // Initialize mods: if mods given - use them, else try use mods from base score, else use empty mod array
            mods ??= baseScore != null ? baseScore.Mods : [];

            perfectPlay.Mods = mods;

            perfectPlay.Accuracy = 1;
            perfectPlay.Passed = true;

            // calculate max combo
            // todo: Get max combo from difficulty calculator instead when diffcalc properly supports lazer-first scores
            perfectPlay.MaxCombo = calculateMaxCombo(beatmap);

            // create statistics assuming all hit objects have perfect hit result
            var statistics = beatmap.HitObjects
                                    .SelectMany(getPerfectHitResults)
                                    .GroupBy(hr => hr, (hr, list) => (hitResult: hr, count: list.Count()))
                                    .ToDictionary(pair => pair.hitResult, pair => pair.count);
            perfectPlay.Statistics = statistics;
            perfectPlay.MaximumStatistics = statistics;

            // calculate total score
            ScoreProcessor scoreProcessor = rulesetInstance.CreateScoreProcessor();
            scoreProcessor.Mods.Value = mods;
            scoreProcessor.ApplyBeatmap(beatmap);
            perfectPlay.TotalScore = scoreProcessor.MaximumTotalScore;

            // compute rank achieved
            // default to SS, then adjust the rank with mods
            perfectPlay.Rank = ScoreRank.X;

            foreach (IApplicableToScoreProcessor mod in mods.OfType<IApplicableToScoreProcessor>())
            {
                perfectPlay.Rank = mod.AdjustRank(perfectPlay.Rank, 1);
            }

            return perfectPlay;
        }

        private static int calculateMaxCombo(IBeatmap beatmap)
        {
            return beatmap.HitObjects.SelectMany(getPerfectHitResults).Count(r => r.AffectsCombo());
        }

        private static IEnumerable<HitResult> getPerfectHitResults(HitObject hitObject)
        {
            foreach (HitObject nested in hitObject.NestedHitObjects)
                yield return nested.Judgement.MaxResult;

            yield return hitObject.Judgement.MaxResult;
        }
    }
}
