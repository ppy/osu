// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Rulesets.Osu.Tests
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
            var beatmap = new OsuBeatmap();
            for (int i = 0; i < maxCombo; i++)
                beatmap.HitObjects.Add(new HitCircle());
            return beatmap;
        }

        protected override IScoringAlgorithm CreateScoreV1(IReadOnlyList<Mod> selectedMods)
            => new ScoreV1(selectedMods)
            {
                ScoreMultiplier = { BindTarget = scoreMultiplier }
            };

        protected override IScoringAlgorithm CreateScoreV2(int maxCombo, IReadOnlyList<Mod> selectedMods)
            => new ScoreV2(maxCombo, selectedMods);

        protected override ProcessorBasedScoringAlgorithm CreateScoreAlgorithm(IBeatmap beatmap, ScoringMode mode, IReadOnlyList<Mod> mods)
            => new OsuProcessorBasedScoringAlgorithm(beatmap, mode, mods);

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
        private const int base_ok = 100;

        private class ScoreV1 : IScoringAlgorithm
        {
            private readonly double modMultiplier;
            public BindableDouble ScoreMultiplier { get; } = new BindableDouble();

            private int currentCombo;

            public ScoreV1(IReadOnlyList<Mod> selectedMods)
            {
                var ruleset = new OsuRuleset();
                modMultiplier = ruleset.CreateLegacyScoreSimulator().GetLegacyScoreMultiplier(selectedMods, new LegacyBeatmapConversionDifficultyInfo
                {
                    SourceRuleset = ruleset.RulesetInfo
                });
            }

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
                TotalScore += (int)(Math.Max(0, currentCombo - 1) * (baseScore / 25 * (ScoreMultiplier.Value * modMultiplier)));

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

            private readonly double modMultiplier;

            private readonly double comboPortionMax;
            private readonly int maxCombo;

            public ScoreV2(int maxCombo, IReadOnlyList<Mod> selectedMods)
            {
                this.maxCombo = maxCombo;

                var ruleset = new OsuRuleset();
                modMultiplier = ruleset.CreateLegacyScoreSimulator().GetLegacyScoreMultiplier(
                    selectedMods.Append(new ModScoreV2()).ToList(),
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

            public void ApplyHit() => applyHitV2(base_great);
            public void ApplyNonPerfect() => applyHitV2(base_ok);

            private void applyHitV2(int baseScore)
            {
                maxBaseScore += base_great;
                currentBaseScore += baseScore;
                comboPortion += baseScore * (1 + ++currentCombo / 10.0);

                currentHits++;
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
                    ((
                        700000 * comboPortion / comboPortionMax +
                        300000 * Math.Pow(accuracy, 10) * ((double)currentHits / maxCombo)
                    ) * modMultiplier);
                }
            }
        }

        private class OsuProcessorBasedScoringAlgorithm : ProcessorBasedScoringAlgorithm
        {
            public OsuProcessorBasedScoringAlgorithm(IBeatmap beatmap, ScoringMode mode, IReadOnlyList<Mod> selectedMods)
                : base(beatmap, mode, selectedMods)
            {
            }

            protected override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor();
            protected override Judgement CreatePerfectJudgementResult() => new OsuJudgement(new HitCircle(), new OsuJudgementCriteria()) { Type = HitResult.Great };
            protected override Judgement CreateNonPerfectJudgementResult() => new OsuJudgement(new HitCircle(), new OsuJudgementCriteria()) { Type = HitResult.Ok };
            protected override Judgement CreateMissJudgementResult() => new OsuJudgement(new HitCircle(), new OsuJudgementCriteria()) { Type = HitResult.Miss };
        }
    }
}
