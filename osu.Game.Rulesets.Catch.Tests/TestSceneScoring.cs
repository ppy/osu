// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneScoring : ScoringTestScene
    {
        public TestSceneScoring()
            : base(supportsNonPerfectJudgements: false)
        {
        }

        private Bindable<double> scoreMultiplier { get; } = new BindableDouble
        {
            Default = 4,
            Value = 4
        };

        protected override IBeatmap CreateBeatmap(int maxCombo)
        {
            var beatmap = new CatchBeatmap();
            for (int i = 0; i < maxCombo; ++i)
                beatmap.HitObjects.Add(new Fruit());
            return beatmap;
        }

        protected override IScoringAlgorithm CreateScoreV1() => new ScoreV1 { ScoreMultiplier = { BindTarget = scoreMultiplier } };

        protected override IScoringAlgorithm CreateScoreV2(int maxCombo) => new ScoreV2(maxCombo);

        protected override ProcessorBasedScoringAlgorithm CreateScoreAlgorithm(IBeatmap beatmap, ScoringMode mode) => new CatchProcessorBasedScoringAlgorithm(beatmap, mode);

        [Test]
        public void TestBasicScenarios()
        {
            AddStep("set up score multiplier", () =>
            {
                scoreMultiplier.BindValueChanged(_ => Rerun());
            });
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
            AddSliderStep("adjust score multiplier", 0, 10, (int)scoreMultiplier.Default, multiplier => scoreMultiplier.Value = multiplier);
        }

        private const int base_great = 300;

        private class ScoreV1 : IScoringAlgorithm
        {
            private int currentCombo;

            public BindableDouble ScoreMultiplier { get; } = new BindableDouble();

            public void ApplyHit() => applyHitV1(base_great);

            public void ApplyNonPerfect() => throw new NotSupportedException("catch does not have \"non-perfect\" judgements.");

            public void ApplyMiss() => applyHitV1(0);

            private void applyHitV1(int baseScore)
            {
                if (baseScore == 0)
                {
                    currentCombo = 0;
                    return;
                }

                TotalScore += baseScore;

                // combo multiplier
                // ReSharper disable once PossibleLossOfFraction
                TotalScore += (int)(Math.Max(0, currentCombo - 1) * (baseScore / 25 * ScoreMultiplier.Value));

                currentCombo++;
            }

            public long TotalScore { get; private set; }
        }

        private class ScoreV2 : IScoringAlgorithm
        {
            private int currentCombo;
            private double comboPortion;

            private readonly double comboPortionMax;

            private const double combo_base = 4;
            private const int combo_cap = 200;

            public ScoreV2(int maxCombo)
            {
                for (int i = 0; i < maxCombo; i++)
                    ApplyHit();

                comboPortionMax = comboPortion;

                currentCombo = 0;
                comboPortion = 0;
            }

            public void ApplyHit() => applyHitV2(base_great);

            public void ApplyNonPerfect() => throw new NotSupportedException("catch does not have \"non-perfect\" judgements.");

            private void applyHitV2(int baseScore)
            {
                comboPortion += baseScore * Math.Min(Math.Max(0.5, Math.Log(++currentCombo, combo_base)), Math.Log(combo_cap, combo_base));
            }

            public void ApplyMiss()
            {
                currentCombo = 0;
            }

            public long TotalScore
                => (int)Math.Round(1000000 * comboPortion / comboPortionMax); // vast simplification, as we're not doing ticks here.
        }

        private class CatchProcessorBasedScoringAlgorithm : ProcessorBasedScoringAlgorithm
        {
            public CatchProcessorBasedScoringAlgorithm(IBeatmap beatmap, ScoringMode mode)
                : base(beatmap, mode)
            {
            }

            protected override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor();

            protected override JudgementResult CreatePerfectJudgementResult() => new CatchJudgementResult(new Fruit(), new CatchJudgement()) { Type = HitResult.Great };

            protected override JudgementResult CreateNonPerfectJudgementResult() => throw new NotSupportedException("catch does not have \"non-perfect\" judgements.");

            protected override JudgementResult CreateMissJudgementResult() => new CatchJudgementResult(new Fruit(), new CatchJudgement()) { Type = HitResult.Miss };
        }
    }
}
