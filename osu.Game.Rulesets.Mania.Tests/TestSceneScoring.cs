// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public partial class TestSceneScoring : ScoringTestScene
    {
        protected override IBeatmap CreateBeatmap(int maxCombo)
        {
            var beatmap = new ManiaBeatmap(new StageDefinition(5));
            for (int i = 0; i < maxCombo; ++i)
                beatmap.HitObjects.Add(new Note());
            return beatmap;
        }

        protected override IScoringAlgorithm CreateScoreV1(IReadOnlyList<Mod> selectedMods) => new ScoreV1(MaxCombo.Value, selectedMods);
        protected override IScoringAlgorithm CreateScoreV2(int maxCombo, IReadOnlyList<Mod> selectedMods) => new ScoreV2(maxCombo, selectedMods);

        protected override ProcessorBasedScoringAlgorithm CreateScoreAlgorithm(IBeatmap beatmap, ScoringMode mode, IReadOnlyList<Mod> selectedMods)
            => new ManiaProcessorBasedScoringAlgorithm(beatmap, mode, selectedMods);

        [Test]
        public void TestBasicScenarios()
        {
            AddStep("set max combo to 100", () => MaxCombo.Value = 100);
            AddStep("set perfect score", () =>
            {
                NonPerfectLocations.Clear();
                MissLocations.Clear();
            });
            AddStep("set score with misses", () =>
            {
                NonPerfectLocations.Clear();
                MissLocations.Clear();
                MissLocations.AddRange(new[] { 24d, 49 });
            });
            AddStep("set score with misses and OKs", () =>
            {
                NonPerfectLocations.Clear();
                MissLocations.Clear();

                NonPerfectLocations.AddRange(new[] { 9d, 19, 29, 39, 59, 69, 79, 89, 99 });
                MissLocations.AddRange(new[] { 24d, 49 });
            });
        }

        private class ScoreV1 : IScoringAlgorithm
        {
            private int currentCombo;
            private double comboAddition = 100;
            private double totalScoreDouble;

            private readonly double scoreMultiplier;

            public ScoreV1(int maxCombo, IReadOnlyList<Mod> selectedMods)
            {
                var ruleset = new ManiaRuleset();

                scoreMultiplier = 500000d / maxCombo * ruleset.CreateLegacyScoreSimulator().GetLegacyScoreMultiplier(selectedMods, new LegacyBeatmapConversionDifficultyInfo
                {
                    SourceRuleset = ruleset.RulesetInfo
                });
            }

            public void ApplyHit() => applyHitV1(320, add => add + 2, 32);
            public void ApplyNonPerfect() => applyHitV1(100, add => add - 24, 8);
            public void ApplyMiss() => applyHitV1(0, _ => -56, 0);

            private void applyHitV1(int scoreIncrease, Func<double, double> comboAdditionFunc, int delta)
            {
                comboAddition = comboAdditionFunc(comboAddition);
                if (currentCombo != 0 && currentCombo % 384 == 0)
                    comboAddition = 100;
                comboAddition = Math.Max(0, Math.Min(comboAddition, 100));
                double scoreIncreaseD = Math.Sqrt(comboAddition) * delta * scoreMultiplier / 320;

                TotalScore = (long)totalScoreDouble;

                scoreIncreaseD += scoreIncrease * scoreMultiplier / 320;
                scoreIncrease = (int)scoreIncreaseD;

                TotalScore += scoreIncrease;
                totalScoreDouble += scoreIncreaseD;

                if (scoreIncrease > 0)
                    currentCombo++;
            }

            public long TotalScore { get; private set; }
        }

        private class ScoreV2 : IScoringAlgorithm
        {
            private int currentCombo;
            private double comboPortion;
            private double currentBaseScore;
            private double maxBaseScore;
            private int currentHits;

            private readonly double comboPortionMax;
            private readonly int maxCombo;
            private readonly double modMultiplier;

            private const double combo_base = 4;

            public ScoreV2(int maxCombo, IReadOnlyList<Mod> selectedMods)
            {
                this.maxCombo = maxCombo;

                var ruleset = new ManiaRuleset();
                modMultiplier = new ManiaRuleset().CreateLegacyScoreSimulator().GetLegacyScoreMultiplier(
                    selectedMods.Append(new ModScoreV2()).ToArray(),
                    new LegacyBeatmapConversionDifficultyInfo
                    {
                        SourceRuleset = ruleset.RulesetInfo
                    });

                for (int i = 0; i < this.maxCombo; i++)
                    ApplyHit();

                comboPortionMax = comboPortion;

                currentCombo = 0;
                comboPortion = 0;
                currentBaseScore = 0;
                maxBaseScore = 0;
                currentHits = 0;
            }

            public void ApplyHit() => applyHitV2(305, 300);
            public void ApplyNonPerfect() => applyHitV2(100, 100);

            private void applyHitV2(int hitValue, int baseHitValue)
            {
                maxBaseScore += 305;
                currentBaseScore += hitValue;
                comboPortion += baseHitValue * Math.Min(Math.Max(0.5, Math.Log(++currentCombo, combo_base)), Math.Log(400, combo_base));

                currentHits++;
            }

            public void ApplyMiss()
            {
                currentHits++;
                maxBaseScore += 305;
                currentCombo = 0;
            }

            public long TotalScore
            {
                get
                {
                    float accuracy = (float)(currentBaseScore / maxBaseScore);

                    return (int)Math.Round
                    ((
                        200000 * comboPortion / comboPortionMax +
                        800000 * Math.Pow(accuracy, 2 + 2 * accuracy) * ((double)currentHits / maxCombo)
                    ) * modMultiplier);
                }
            }
        }

        private class ManiaProcessorBasedScoringAlgorithm : ProcessorBasedScoringAlgorithm
        {
            public ManiaProcessorBasedScoringAlgorithm(IBeatmap beatmap, ScoringMode mode, IReadOnlyList<Mod> selectedMods)
                : base(beatmap, mode, selectedMods)
            {
            }

            protected override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor();

            protected override Judgement CreatePerfectJudgementResult() => new Judgement(new Note(), new ManiaJudgementCriteria()) { Type = HitResult.Perfect };

            protected override Judgement CreateNonPerfectJudgementResult() => new Judgement(new Note(), new ManiaJudgementCriteria()) { Type = HitResult.Ok };

            protected override Judgement CreateMissJudgementResult() => new Judgement(new Note(), new ManiaJudgementCriteria()) { Type = HitResult.Miss };
        }
    }
}
