// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public partial class TestSceneScoring : ScoringTestScene
    {
        private Bindable<double> scoreMultiplier { get; } = new BindableDouble
        {
            Default = 4,
            Value = 4
        };

        protected override IBeatmap CreateBeatmap(int maxCombo)
        {
            var beatmap = new TaikoBeatmap();
            for (int i = 0; i < maxCombo; ++i)
                beatmap.HitObjects.Add(new Hit());
            return beatmap;
        }

        protected override IScoringAlgorithm CreateScoreV1() => new ScoreV1 { ScoreMultiplier = { BindTarget = scoreMultiplier } };
        protected override IScoringAlgorithm CreateScoreV2(int maxCombo) => new ScoreV2(maxCombo);

        protected override ProcessorBasedScoringAlgorithm CreateScoreAlgorithm(IBeatmap beatmap, ScoringMode mode) => new TaikoProcessorBasedScoringAlgorithm(beatmap, mode);

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
            AddStep("set score with misses and OKs", () =>
            {
                NonPerfectLocations.Clear();
                MissLocations.Clear();

                NonPerfectLocations.AddRange(new[] { 9d, 19, 29, 39, 59, 69, 79, 89, 99 });
                MissLocations.AddRange(new[] { 24d, 49 });
            });
            AddSliderStep("adjust score multiplier", 0, 10, (int)scoreMultiplier.Default, multiplier => scoreMultiplier.Value = multiplier);
        }

        private const int base_great = 300;
        private const int base_ok = 150;

        private class ScoreV1 : IScoringAlgorithm
        {
            private int currentCombo;

            public BindableDouble ScoreMultiplier { get; } = new BindableDouble();

            public void ApplyHit() => applyHitV1(base_great);

            public void ApplyNonPerfect() => applyHitV1(base_ok);

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
                TotalScore += (int)((baseScore / 35) * 2 * (ScoreMultiplier.Value + 1)) * (Math.Min(100, currentCombo) / 10);

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

            private const double combo_base = 4;

            public ScoreV2(int maxCombo)
            {
                this.maxCombo = maxCombo;

                for (int i = 0; i < this.maxCombo; i++)
                    ApplyHit();

                comboPortionMax = comboPortion;

                currentCombo = 0;
                comboPortion = 0;
                currentBaseScore = 0;
                maxBaseScore = 0;
                currentHits = 0;
            }

            public void ApplyHit() => applyHitV2(base_great);

            public void ApplyNonPerfect() => applyHitV2(base_ok);

            private void applyHitV2(int baseScore)
            {
                maxBaseScore += base_great;
                currentBaseScore += baseScore;

                currentHits++;

                // `base_great` is INTENTIONALLY used above here instead of `baseScore`
                // see `BaseHitValue` override in `ScoreChangeTaiko` on stable
                comboPortion += base_great * Math.Min(Math.Max(0.5, Math.Log(++currentCombo, combo_base)), Math.Log(400, combo_base));
            }

            public void ApplyMiss()
            {
                currentHits++;
                maxBaseScore += base_great;
                currentCombo = 0;
            }

            public long TotalScore
            {
                get
                {
                    double accuracy = currentBaseScore / maxBaseScore;

                    return (int)Math.Round
                    (
                        250000 * comboPortion / comboPortionMax +
                        750000 * Math.Pow(accuracy, 3.6) * ((double)currentHits / maxCombo)
                    );
                }
            }
        }

        private class TaikoProcessorBasedScoringAlgorithm : ProcessorBasedScoringAlgorithm
        {
            public TaikoProcessorBasedScoringAlgorithm(IBeatmap beatmap, ScoringMode mode)
                : base(beatmap, mode)
            {
            }

            protected override ScoreProcessor CreateScoreProcessor() => new TaikoScoreProcessor();
            protected override JudgementResult CreatePerfectJudgementResult() => new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Great };
            protected override JudgementResult CreateNonPerfectJudgementResult() => new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Ok };
            protected override JudgementResult CreateMissJudgementResult() => new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Miss };
        }
    }
}
