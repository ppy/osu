// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring.Legacy;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Rulesets.Scoring
{
    public partial class ScoreProcessorTest
    {
        private ScoreProcessor scoreProcessor;
        private IBeatmap beatmap;

        [SetUp]
        public void SetUp()
        {
            scoreProcessor = new ScoreProcessor(new OsuRuleset());
            beatmap = new TestBeatmap(new RulesetInfo())
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle()
                }
            };
        }

        [TestCase(ScoringMode.Standardised, HitResult.Meh, 116_667)]
        [TestCase(ScoringMode.Standardised, HitResult.Ok, 233_338)]
        [TestCase(ScoringMode.Standardised, HitResult.Great, 1_000_000)]
        [TestCase(ScoringMode.Classic, HitResult.Meh, 0)]
        [TestCase(ScoringMode.Classic, HitResult.Ok, 2)]
        [TestCase(ScoringMode.Classic, HitResult.Great, 36)]
        public void TestSingleOsuHit(ScoringMode scoringMode, HitResult hitResult, int expectedScore)
        {
            scoreProcessor.ApplyBeatmap(beatmap);

            var judgementResult = new JudgementResult(beatmap.HitObjects.Single(), new OsuJudgement())
            {
                Type = hitResult
            };
            scoreProcessor.ApplyResult(judgementResult);

            Assert.That(scoreProcessor.GetDisplayScore(scoringMode), Is.EqualTo(expectedScore).Within(0.5d));
        }

        /// <summary>
        /// Test to see that all <see cref="HitResult"/>s contribute to score portions in correct amounts.
        /// </summary>
        /// <param name="scoringMode">Scoring mode to test.</param>
        /// <param name="hitResult">The <see cref="HitResult"/> that will be applied to selected hit objects.</param>
        /// <param name="maxResult">The maximum <see cref="HitResult"/> achievable.</param>
        /// <param name="expectedScore">Expected score after all objects have been judged, rounded to the nearest integer.</param>
        /// <remarks>
        /// This test intentionally misses the 3rd hitobject to achieve lower than 75% accuracy and 50% max combo.
        /// </remarks>
        [TestCase(ScoringMode.Standardised, HitResult.Miss, HitResult.Great, 0)]
        [TestCase(ScoringMode.Standardised, HitResult.Meh, HitResult.Great, 79_333)]
        [TestCase(ScoringMode.Standardised, HitResult.Ok, HitResult.Great, 158_667)]
        [TestCase(ScoringMode.Standardised, HitResult.Good, HitResult.Perfect, 302_402)]
        [TestCase(ScoringMode.Standardised, HitResult.Great, HitResult.Great, 492_894)]
        [TestCase(ScoringMode.Standardised, HitResult.Perfect, HitResult.Perfect, 492_894)]
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickMiss, HitResult.SmallTickHit, 0)]
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickHit, HitResult.SmallTickHit, 541_894)]
        [TestCase(ScoringMode.Standardised, HitResult.LargeTickMiss, HitResult.LargeTickHit, 0)]
        [TestCase(ScoringMode.Standardised, HitResult.LargeTickHit, HitResult.LargeTickHit, 492_894)]
        [TestCase(ScoringMode.Standardised, HitResult.SmallBonus, HitResult.SmallBonus, 1_000_030)]
        [TestCase(ScoringMode.Standardised, HitResult.LargeBonus, HitResult.LargeBonus, 1_000_150)]
        [TestCase(ScoringMode.Classic, HitResult.Miss, HitResult.Great, 0)]
        [TestCase(ScoringMode.Classic, HitResult.Meh, HitResult.Great, 4)]
        [TestCase(ScoringMode.Classic, HitResult.Ok, HitResult.Great, 15)]
        [TestCase(ScoringMode.Classic, HitResult.Good, HitResult.Perfect, 53)]
        [TestCase(ScoringMode.Classic, HitResult.Great, HitResult.Great, 140)]
        [TestCase(ScoringMode.Classic, HitResult.Perfect, HitResult.Perfect, 140)]
        [TestCase(ScoringMode.Classic, HitResult.SmallTickMiss, HitResult.SmallTickHit, 0)]
        [TestCase(ScoringMode.Classic, HitResult.SmallTickHit, HitResult.SmallTickHit, 11)]
        [TestCase(ScoringMode.Classic, HitResult.LargeTickMiss, HitResult.LargeTickHit, 0)]
        [TestCase(ScoringMode.Classic, HitResult.LargeTickHit, HitResult.LargeTickHit, 9)]
        [TestCase(ScoringMode.Classic, HitResult.SmallBonus, HitResult.SmallBonus, 36)]
        [TestCase(ScoringMode.Classic, HitResult.LargeBonus, HitResult.LargeBonus, 36)]
        public void TestFourVariousResultsOneMiss(ScoringMode scoringMode, HitResult hitResult, HitResult maxResult, int expectedScore)
        {
            var minResult = new TestJudgement(hitResult).MinResult;

            IBeatmap fourObjectBeatmap = new TestBeatmap(new RulesetInfo())
            {
                HitObjects = new List<HitObject>(Enumerable.Repeat(new TestHitObject(maxResult), 4))
            };
            scoreProcessor.ApplyBeatmap(fourObjectBeatmap);

            for (int i = 0; i < 4; i++)
            {
                var judgementResult = new JudgementResult(fourObjectBeatmap.HitObjects[i], new TestJudgement(maxResult))
                {
                    Type = i == 2 ? minResult : hitResult
                };
                scoreProcessor.ApplyResult(judgementResult);
            }

            Assert.That(scoreProcessor.GetDisplayScore(scoringMode), Is.EqualTo(expectedScore).Within(0.5d));
        }

        [Test]
        public void TestEmptyBeatmap(
            [Values(ScoringMode.Standardised, ScoringMode.Classic)]
            ScoringMode scoringMode)
        {
            scoreProcessor.ApplyBeatmap(new TestBeatmap(new RulesetInfo()));

            Assert.That(scoreProcessor.GetDisplayScore(scoringMode), Is.Zero);
        }

        [TestCase(HitResult.IgnoreHit, HitResult.IgnoreMiss)]
        [TestCase(HitResult.Meh, HitResult.Miss)]
        [TestCase(HitResult.Ok, HitResult.Miss)]
        [TestCase(HitResult.Good, HitResult.Miss)]
        [TestCase(HitResult.Great, HitResult.Miss)]
        [TestCase(HitResult.Perfect, HitResult.Miss)]
        [TestCase(HitResult.SmallTickHit, HitResult.SmallTickMiss)]
        [TestCase(HitResult.LargeTickHit, HitResult.LargeTickMiss)]
        [TestCase(HitResult.SmallBonus, HitResult.IgnoreMiss)]
        [TestCase(HitResult.LargeBonus, HitResult.IgnoreMiss)]
        public void TestMinResults(HitResult hitResult, HitResult expectedMinResult)
        {
            Assert.AreEqual(expectedMinResult, new TestJudgement(hitResult).MinResult);
        }

        [TestCase(HitResult.None, false)]
        [TestCase(HitResult.IgnoreMiss, false)]
        [TestCase(HitResult.IgnoreHit, false)]
        [TestCase(HitResult.Miss, true)]
        [TestCase(HitResult.Meh, true)]
        [TestCase(HitResult.Ok, true)]
        [TestCase(HitResult.Good, true)]
        [TestCase(HitResult.Great, true)]
        [TestCase(HitResult.Perfect, true)]
        [TestCase(HitResult.SmallTickMiss, false)]
        [TestCase(HitResult.SmallTickHit, false)]
        [TestCase(HitResult.LargeTickMiss, true)]
        [TestCase(HitResult.LargeTickHit, true)]
        [TestCase(HitResult.SmallBonus, false)]
        [TestCase(HitResult.LargeBonus, false)]
        public void TestAffectsCombo(HitResult hitResult, bool expectedReturnValue)
        {
            Assert.AreEqual(expectedReturnValue, hitResult.AffectsCombo());
        }

        [TestCase(HitResult.None, false)]
        [TestCase(HitResult.IgnoreMiss, false)]
        [TestCase(HitResult.IgnoreHit, false)]
        [TestCase(HitResult.Miss, true)]
        [TestCase(HitResult.Meh, true)]
        [TestCase(HitResult.Ok, true)]
        [TestCase(HitResult.Good, true)]
        [TestCase(HitResult.Great, true)]
        [TestCase(HitResult.Perfect, true)]
        [TestCase(HitResult.SmallTickMiss, true)]
        [TestCase(HitResult.SmallTickHit, true)]
        [TestCase(HitResult.LargeTickMiss, true)]
        [TestCase(HitResult.LargeTickHit, true)]
        [TestCase(HitResult.SmallBonus, false)]
        [TestCase(HitResult.LargeBonus, false)]
        public void TestAffectsAccuracy(HitResult hitResult, bool expectedReturnValue)
        {
            Assert.AreEqual(expectedReturnValue, hitResult.AffectsAccuracy());
        }

        [TestCase(HitResult.None, false)]
        [TestCase(HitResult.IgnoreMiss, false)]
        [TestCase(HitResult.IgnoreHit, false)]
        [TestCase(HitResult.Miss, false)]
        [TestCase(HitResult.Meh, false)]
        [TestCase(HitResult.Ok, false)]
        [TestCase(HitResult.Good, false)]
        [TestCase(HitResult.Great, false)]
        [TestCase(HitResult.Perfect, false)]
        [TestCase(HitResult.SmallTickMiss, false)]
        [TestCase(HitResult.SmallTickHit, false)]
        [TestCase(HitResult.LargeTickMiss, false)]
        [TestCase(HitResult.LargeTickHit, false)]
        [TestCase(HitResult.SmallBonus, true)]
        [TestCase(HitResult.LargeBonus, true)]
        public void TestIsBonus(HitResult hitResult, bool expectedReturnValue)
        {
            Assert.AreEqual(expectedReturnValue, hitResult.IsBonus());
        }

        [TestCase(HitResult.None, false)]
        [TestCase(HitResult.IgnoreMiss, false)]
        [TestCase(HitResult.IgnoreHit, true)]
        [TestCase(HitResult.Miss, false)]
        [TestCase(HitResult.Meh, true)]
        [TestCase(HitResult.Ok, true)]
        [TestCase(HitResult.Good, true)]
        [TestCase(HitResult.Great, true)]
        [TestCase(HitResult.Perfect, true)]
        [TestCase(HitResult.SmallTickMiss, false)]
        [TestCase(HitResult.SmallTickHit, true)]
        [TestCase(HitResult.LargeTickMiss, false)]
        [TestCase(HitResult.LargeTickHit, true)]
        [TestCase(HitResult.SmallBonus, true)]
        [TestCase(HitResult.LargeBonus, true)]
        public void TestIsHit(HitResult hitResult, bool expectedReturnValue)
        {
            Assert.AreEqual(expectedReturnValue, hitResult.IsHit());
        }

        [TestCase(HitResult.None, false)]
        [TestCase(HitResult.IgnoreMiss, false)]
        [TestCase(HitResult.IgnoreHit, false)]
        [TestCase(HitResult.Miss, true)]
        [TestCase(HitResult.Meh, true)]
        [TestCase(HitResult.Ok, true)]
        [TestCase(HitResult.Good, true)]
        [TestCase(HitResult.Great, true)]
        [TestCase(HitResult.Perfect, true)]
        [TestCase(HitResult.SmallTickMiss, true)]
        [TestCase(HitResult.SmallTickHit, true)]
        [TestCase(HitResult.LargeTickMiss, true)]
        [TestCase(HitResult.LargeTickHit, true)]
        [TestCase(HitResult.SmallBonus, true)]
        [TestCase(HitResult.LargeBonus, true)]
        public void TestIsScorable(HitResult hitResult, bool expectedReturnValue)
        {
            Assert.AreEqual(expectedReturnValue, hitResult.IsScorable());
        }

#pragma warning disable CS0618
        [Test]
        public void TestLegacyComboIncrease()
        {
            Assert.That(HitResult.LegacyComboIncrease.IncreasesCombo(), Is.True);
            Assert.That(HitResult.LegacyComboIncrease.BreaksCombo(), Is.False);
            Assert.That(HitResult.LegacyComboIncrease.AffectsCombo(), Is.True);
            Assert.That(HitResult.LegacyComboIncrease.AffectsAccuracy(), Is.False);
            Assert.That(HitResult.LegacyComboIncrease.IsBasic(), Is.False);
            Assert.That(HitResult.LegacyComboIncrease.IsTick(), Is.False);
            Assert.That(HitResult.LegacyComboIncrease.IsBonus(), Is.False);
            Assert.That(HitResult.LegacyComboIncrease.IsHit(), Is.True);
            Assert.That(HitResult.LegacyComboIncrease.IsScorable(), Is.True);
            Assert.That(HitResultExtensions.ALL_TYPES, Does.Not.Contain(HitResult.LegacyComboIncrease));
        }
#pragma warning restore CS0618

        [Test]
        public void TestAccuracyWhenNearPerfect()
        {
            const int count_judgements = 1000;
            const int count_misses = 1;

            beatmap = new TestBeatmap(new RulesetInfo())
            {
                HitObjects = new List<HitObject>(Enumerable.Repeat(new TestHitObject(HitResult.Great), count_judgements))
            };

            scoreProcessor = new TestScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[i], new TestJudgement(HitResult.Great))
                {
                    Type = i == 0 ? HitResult.Miss : HitResult.Great
                });
            }

            const double expected = (count_judgements - count_misses) / (double)count_judgements;
            double actual = scoreProcessor.Accuracy.Value;

            Assert.That(actual, Is.Not.EqualTo(0.0));
            Assert.That(actual, Is.Not.EqualTo(1.0));
            Assert.That(actual, Is.EqualTo(expected).Within(Precision.FLOAT_EPSILON));
        }

        private class TestJudgement : Judgement
        {
            public override HitResult MaxResult { get; }

            public TestJudgement(HitResult maxResult)
            {
                MaxResult = maxResult;
            }
        }

        private class TestHitObject : HitObject
        {
            private readonly HitResult maxResult;

            public override Judgement CreateJudgement()
            {
                return new TestJudgement(maxResult);
            }

            public TestHitObject(HitResult maxResult)
            {
                this.maxResult = maxResult;
            }
        }

        private partial class TestScoreProcessor : ScoreProcessor
        {
            public TestScoreProcessor()
                : base(new TestRuleset())
            {
            }

            protected override double ComputeTotalScore(double comboProgress, double accuracyProgress, double bonusPortion)
            {
                return 500000 * comboProgress +
                       500000 * Accuracy.Value * accuracyProgress +
                       bonusPortion;
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            private class TestRuleset : Ruleset
            {
                protected override IEnumerable<HitResult> GetValidHitResults() => new[] { HitResult.Great };

                public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();

                public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => throw new NotImplementedException();

                public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();

                public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();

                public override string Description => string.Empty;
                public override string ShortName => string.Empty;
            }
        }
    }
}
