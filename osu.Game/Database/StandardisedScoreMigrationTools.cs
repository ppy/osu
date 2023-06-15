// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public static class StandardisedScoreMigrationTools
    {
        public static bool ShouldMigrateToNewStandardised(ScoreInfo score)
        {
            if (score.IsLegacyScore)
                return false;

            // Recalculate the old-style standardised score to see if this was an old lazer score.
            bool oldScoreMatchesExpectations = GetOldStandardised(score) == score.TotalScore;
            // Some older scores don't have correct statistics populated, so let's give them benefit of doubt.
            bool scoreIsVeryOld = score.Date < new DateTime(2023, 1, 1, 0, 0, 0);

            return oldScoreMatchesExpectations || scoreIsVeryOld;
        }

        public static long GetNewStandardised(ScoreInfo score)
        {
            int maxJudgementIndex = 0;

            // Avoid retrieving from realm inside loops.
            int maxCombo = score.MaxCombo;

            var ruleset = score.Ruleset.CreateInstance();
            var processor = ruleset.CreateScoreProcessor();

            processor.TrackHitEvents = false;

            var beatmap = new Beatmap();

            HitResult maxRulesetJudgement = ruleset.GetHitResults().First().result;

            // This is a list of all results, ordered from best to worst.
            // We are constructing a "best possible" score from the statistics provided because it's the best we can do.
            List<HitResult> sortedHits = score.Statistics
                                              .Where(kvp => kvp.Key.AffectsCombo())
                                              .OrderByDescending(kvp => Judgement.ToNumericResult(kvp.Key))
                                              .SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value))
                                              .ToList();

            // Attempt to use maximum statistics from the database.
            var maximumJudgements = score.MaximumStatistics
                                         .Where(kvp => kvp.Key.AffectsCombo())
                                         .OrderByDescending(kvp => Judgement.ToNumericResult(kvp.Key))
                                         .SelectMany(kvp => Enumerable.Repeat(new FakeJudgement(kvp.Key), kvp.Value))
                                         .ToList();

            // Some older scores may not have maximum statistics populated correctly.
            // In this case we need to fill them with best-known-defaults.
            if (maximumJudgements.Count != sortedHits.Count)
            {
                maximumJudgements = sortedHits
                                    .Select(r => new FakeJudgement(getMaxJudgementFor(r, maxRulesetJudgement)))
                                    .ToList();
            }

            // This is required to get the correct maximum combo portion.
            foreach (var judgement in maximumJudgements)
                beatmap.HitObjects.Add(new FakeHit(judgement));
            processor.ApplyBeatmap(beatmap);
            processor.Mods.Value = score.Mods;

            // Insert all misses into a queue.
            // These will be nibbled at whenever we need to reset the combo.
            Queue<HitResult> misses = new Queue<HitResult>(score.Statistics
                                                                .Where(kvp => kvp.Key == HitResult.Miss || kvp.Key == HitResult.LargeTickMiss)
                                                                .SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)));

            foreach (var result in sortedHits)
            {
                // For the main part of this loop, ignore all misses, as they will be inserted from the queue.
                if (result == HitResult.Miss || result == HitResult.LargeTickMiss)
                    continue;

                // Reset combo if required.
                if (processor.Combo.Value == maxCombo)
                    insertMiss();

                processor.ApplyResult(new JudgementResult(null!, maximumJudgements[maxJudgementIndex++])
                {
                    Type = result
                });
            }

            // Ensure we haven't forgotten any misses.
            while (misses.Count > 0)
                insertMiss();

            var bonusHits = score.Statistics
                                 .Where(kvp => kvp.Key.IsBonus())
                                 .SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value));

            foreach (var result in bonusHits)
                processor.ApplyResult(new JudgementResult(null!, new FakeJudgement(result)) { Type = result });

            // Not true for all scores for whatever reason. Oh well.
            // Debug.Assert(processor.HighestCombo.Value == score.MaxCombo);

            return processor.TotalScore.Value;

            void insertMiss()
            {
                if (misses.Count > 0)
                {
                    processor.ApplyResult(new JudgementResult(null!, maximumJudgements[maxJudgementIndex++])
                    {
                        Type = misses.Dequeue(),
                    });
                }
                else
                {
                    // We ran out of misses. But we can't let max combo increase beyond the known value,
                    // so let's forge a miss.
                    processor.ApplyResult(new JudgementResult(null!, new FakeJudgement(getMaxJudgementFor(HitResult.Miss, maxRulesetJudgement)))
                    {
                        Type = HitResult.Miss,
                    });
                }
            }
        }

        private static HitResult getMaxJudgementFor(HitResult hitResult, HitResult max)
        {
            switch (hitResult)
            {
                case HitResult.Miss:
                case HitResult.Meh:
                case HitResult.Ok:
                case HitResult.Good:
                case HitResult.Great:
                case HitResult.Perfect:
                    return max;

                case HitResult.SmallTickMiss:
                case HitResult.SmallTickHit:
                    return HitResult.SmallTickHit;

                case HitResult.LargeTickMiss:
                case HitResult.LargeTickHit:
                    return HitResult.LargeTickHit;
            }

            return HitResult.IgnoreHit;
        }

        public static long GetOldStandardised(ScoreInfo score)
        {
            double accuracyScore =
                (double)score.Statistics.Where(kvp => kvp.Key.AffectsAccuracy()).Sum(kvp => Judgement.ToNumericResult(kvp.Key) * kvp.Value)
                / score.MaximumStatistics.Where(kvp => kvp.Key.AffectsAccuracy()).Sum(kvp => Judgement.ToNumericResult(kvp.Key) * kvp.Value);
            double comboScore = (double)score.MaxCombo / score.MaximumStatistics.Where(kvp => kvp.Key.AffectsCombo()).Sum(kvp => kvp.Value);
            double bonusScore = score.Statistics.Where(kvp => kvp.Key.IsBonus()).Sum(kvp => Judgement.ToNumericResult(kvp.Key) * kvp.Value);

            double accuracyPortion = 0.3;

            switch (score.RulesetID)
            {
                case 1:
                    accuracyPortion = 0.75;
                    break;

                case 3:
                    accuracyPortion = 0.99;
                    break;
            }

            double modMultiplier = 1;

            foreach (var mod in score.Mods)
                modMultiplier *= mod.ScoreMultiplier;

            return (long)Math.Round((1000000 * (accuracyPortion * accuracyScore + (1 - accuracyPortion) * comboScore) + bonusScore) * modMultiplier);
        }

        private class FakeHit : HitObject
        {
            private readonly Judgement judgement;

            public override Judgement CreateJudgement() => judgement;

            public FakeHit(Judgement judgement)
            {
                this.judgement = judgement;
            }
        }

        private class FakeJudgement : Judgement
        {
            public override HitResult MaxResult { get; }

            public FakeJudgement(HitResult maxResult)
            {
                MaxResult = maxResult;
            }
        }
    }
}
