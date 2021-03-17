// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Rulesets.Scoring
{
    public class ScoreProcessorTest
    {
        private ScoreProcessor scoreProcessor;
        private IBeatmap beatmap;

        [SetUp]
        public void SetUp()
        {
            scoreProcessor = new ScoreProcessor();
            beatmap = new TestBeatmap(new RulesetInfo())
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle()
                }
            };
        }

        [TestCase(ScoringMode.Standardised, HitResult.Meh, 750_000)]
        [TestCase(ScoringMode.Standardised, HitResult.Ok, 800_000)]
        [TestCase(ScoringMode.Standardised, HitResult.Great, 1_000_000)]
        [TestCase(ScoringMode.Classic, HitResult.Meh, 50)]
        [TestCase(ScoringMode.Classic, HitResult.Ok, 100)]
        [TestCase(ScoringMode.Classic, HitResult.Great, 300)]
        public void TestSingleOsuHit(ScoringMode scoringMode, HitResult hitResult, int expectedScore)
        {
            scoreProcessor.Mode.Value = scoringMode;
            scoreProcessor.ApplyBeatmap(beatmap);

            var judgementResult = new JudgementResult(beatmap.HitObjects.Single(), new OsuJudgement())
            {
                Type = hitResult
            };
            scoreProcessor.ApplyResult(judgementResult);

            Assert.IsTrue(Precision.AlmostEquals(expectedScore, scoreProcessor.TotalScore.Value));
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
        /// <para>
        /// For standardised scoring, <paramref name="expectedScore"/> is calculated using the following formula:
        /// 1_000_000 * (((3 * <paramref name="hitResult"/>) / (4 * <paramref name="maxResult"/>)) * 30% + (bestCombo / maxCombo) * 70%)
        /// </para>
        /// <para>
        /// For classic scoring, <paramref name="expectedScore"/> is calculated using the following formula:
        /// <paramref name="hitResult"/> / <paramref name="maxResult"/> * 936
        /// where 936 is simplified from:
        /// 75% * 4 * 300 * (1 + 1/25)
        /// </para>
        /// </remarks>
        [TestCase(ScoringMode.Standardised, HitResult.Miss, HitResult.Great, 0)] // (3 * 0) / (4 * 300) * 300_000 + (0 / 4) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.Meh, HitResult.Great, 387_500)] // (3 * 50) / (4 * 300) * 300_000 + (2 / 4) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.Ok, HitResult.Great, 425_000)] // (3 * 100) / (4 * 300) * 300_000 + (2 / 4) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.Good, HitResult.Perfect, 478_571)] // (3 * 200) / (4 * 350) * 300_000 + (2 / 4) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.Great, HitResult.Great, 575_000)] // (3 * 300) / (4 * 300) * 300_000 + (2 / 4) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.Perfect, HitResult.Perfect, 575_000)] // (3 * 350) / (4 * 350) * 300_000 + (2 / 4) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickMiss, HitResult.SmallTickHit, 700_000)] // (3 * 0) / (4 * 10) * 300_000 + 700_000 (max combo 0)
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickHit, HitResult.SmallTickHit, 925_000)] // (3 * 10) / (4 * 10) * 300_000 + 700_000 (max combo 0)
        [TestCase(ScoringMode.Standardised, HitResult.LargeTickMiss, HitResult.LargeTickHit, 0)] // (3 * 0) / (4 * 30) * 300_000 + (0 / 4) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.LargeTickHit, HitResult.LargeTickHit, 575_000)] // (3 * 30) / (4 * 30) * 300_000 + (0 / 4) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.SmallBonus, HitResult.SmallBonus, 700_030)] // 0 * 300_000 + 700_000 (max combo 0) + 3 * 10 (bonus points)
        [TestCase(ScoringMode.Standardised, HitResult.LargeBonus, HitResult.LargeBonus, 700_150)] // 0 * 300_000 + 700_000 (max combo 0) + 3 * 50 (bonus points)
        [TestCase(ScoringMode.Classic, HitResult.Miss, HitResult.Great, 0)] // (0 * 4 * 300) * (1 + 0 / 25)
        [TestCase(ScoringMode.Classic, HitResult.Meh, HitResult.Great, 156)] // (((3 * 50) / (4 * 300)) * 4 * 300) * (1 + 1 / 25)
        [TestCase(ScoringMode.Classic, HitResult.Ok, HitResult.Great, 312)] // (((3 * 100) / (4 * 300)) * 4 * 300) * (1 + 1 / 25)
        [TestCase(ScoringMode.Classic, HitResult.Good, HitResult.Perfect, 535)] // (((3 * 200) / (4 * 350)) * 4 * 300) * (1 + 1 / 25)
        [TestCase(ScoringMode.Classic, HitResult.Great, HitResult.Great, 936)] // (((3 * 300) / (4 * 300)) * 4 * 300) * (1 + 1 / 25)
        [TestCase(ScoringMode.Classic, HitResult.Perfect, HitResult.Perfect, 936)] // (((3 * 350) / (4 * 350)) * 4 * 300) * (1 + 1 / 25)
        [TestCase(ScoringMode.Classic, HitResult.SmallTickMiss, HitResult.SmallTickHit, 0)] // (0 * 1 * 300) * (1 + 0 / 25)
        [TestCase(ScoringMode.Classic, HitResult.SmallTickHit, HitResult.SmallTickHit, 225)] // (((3 * 10) / (4 * 10)) * 1 * 300) * (1 + 0 / 25)
        [TestCase(ScoringMode.Classic, HitResult.LargeTickMiss, HitResult.LargeTickHit, 0)] // (0 * 4 * 300) * (1 + 0 / 25)
        [TestCase(ScoringMode.Classic, HitResult.LargeTickHit, HitResult.LargeTickHit, 936)] // (((3 * 50) / (4 * 50)) * 4 * 300) * (1 + 1 / 25)
        [TestCase(ScoringMode.Classic, HitResult.SmallBonus, HitResult.SmallBonus, 30)] // (0 * 1 * 300) * (1 + 0 / 25) + 3 * 10 (bonus points)
        [TestCase(ScoringMode.Classic, HitResult.LargeBonus, HitResult.LargeBonus, 150)] // (0 * 1 * 300) * (1 + 0 / 25) * 3 * 50 (bonus points)
        public void TestFourVariousResultsOneMiss(ScoringMode scoringMode, HitResult hitResult, HitResult maxResult, int expectedScore)
        {
            var minResult = new TestJudgement(hitResult).MinResult;

            IBeatmap fourObjectBeatmap = new TestBeatmap(new RulesetInfo())
            {
                HitObjects = new List<HitObject>(Enumerable.Repeat(new TestHitObject(maxResult), 4))
            };
            scoreProcessor.Mode.Value = scoringMode;
            scoreProcessor.ApplyBeatmap(fourObjectBeatmap);

            for (int i = 0; i < 4; i++)
            {
                var judgementResult = new JudgementResult(fourObjectBeatmap.HitObjects[i], new Judgement())
                {
                    Type = i == 2 ? minResult : hitResult
                };
                scoreProcessor.ApplyResult(judgementResult);
            }

            Assert.IsTrue(Precision.AlmostEquals(expectedScore, scoreProcessor.TotalScore.Value, 0.5));
        }

        /// <remarks>
        /// This test uses a beatmap with four small ticks and one object with the <see cref="Judgement.MaxResult"/> of <see cref="HitResult.Ok"/>.
        /// Its goal is to ensure that with the <see cref="ScoringMode"/> of <see cref="ScoringMode.Standardised"/>,
        /// small ticks contribute to the accuracy portion, but not the combo portion.
        /// In contrast, <see cref="ScoringMode.Classic"/> does not have separate combo and accuracy portion (they are multiplied by each other).
        /// </remarks>
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickHit, 978_571)] // (3 * 10 + 100) / (4 * 10 + 100) * 300_000 + (1 / 1) * 700_000
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickMiss, 914_286)] // (3 * 0 + 100) / (4 * 10 + 100) * 300_000 + (1 / 1) * 700_000
        [TestCase(ScoringMode.Classic, HitResult.SmallTickHit, 279)] // (((3 * 10 + 100) / (4 * 10 + 100)) * 1 * 300) * (1 + 0 / 25)
        [TestCase(ScoringMode.Classic, HitResult.SmallTickMiss, 214)] // (((3 * 0 + 100) / (4 * 10 + 100)) * 1 * 300) * (1 + 0 / 25)
        public void TestSmallTicksAccuracy(ScoringMode scoringMode, HitResult hitResult, int expectedScore)
        {
            IEnumerable<HitObject> hitObjects = Enumerable
                                                .Repeat(new TestHitObject(HitResult.SmallTickHit), 4)
                                                .Append(new TestHitObject(HitResult.Ok));
            IBeatmap fiveObjectBeatmap = new TestBeatmap(new RulesetInfo())
            {
                HitObjects = hitObjects.ToList()
            };
            scoreProcessor.Mode.Value = scoringMode;
            scoreProcessor.ApplyBeatmap(fiveObjectBeatmap);

            for (int i = 0; i < 4; i++)
            {
                var judgementResult = new JudgementResult(fiveObjectBeatmap.HitObjects[i], new Judgement())
                {
                    Type = i == 2 ? HitResult.SmallTickMiss : hitResult
                };
                scoreProcessor.ApplyResult(judgementResult);
            }

            var lastJudgementResult = new JudgementResult(fiveObjectBeatmap.HitObjects.Last(), new Judgement())
            {
                Type = HitResult.Ok
            };
            scoreProcessor.ApplyResult(lastJudgementResult);

            Assert.IsTrue(Precision.AlmostEquals(expectedScore, scoreProcessor.TotalScore.Value, 0.5));
        }

        [Test]
        public void TestEmptyBeatmap(
            [Values(ScoringMode.Standardised, ScoringMode.Classic)]
            ScoringMode scoringMode)
        {
            scoreProcessor.Mode.Value = scoringMode;
            scoreProcessor.ApplyBeatmap(new TestBeatmap(new RulesetInfo()));

            Assert.IsTrue(Precision.AlmostEquals(0, scoreProcessor.TotalScore.Value));
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
    }
}
