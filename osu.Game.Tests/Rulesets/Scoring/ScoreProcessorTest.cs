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
        /// Test to see that all <see cref="HitResult"/> contribute to score portions in correct amounts.
        /// </summary>
        /// <param name="scoringMode">Scoring mode to test</param>
        /// <param name="hitResult">HitResult that will be applied to HitObjects</param>
        /// <param name="maxResult">HitResult used for accuracy calcualtion</param>
        /// <param name="expectedScore">Expected score after 3/4 hitobjects have been hit rounded to nearest integer</param>
        /// <remarks>
        /// This test intentionally misses the 3rd hitobject to achieve lower than 75% accuracy and 50% max combo
        /// expectedScore is calcualted using this algorithm for standardised scoring: 1_000_000 * ((75% * score_per_hitobject / max_per_hitobject * 30%) + (50% * 70%))
        /// "75% * score_per_hitobject / max_per_hitobject" is the accuracy we would get for hitting 3/4 hitobjects that award "score_per_hitobject / max_per_hitobject" accuracy each hit
        ///
        /// expectedScore is calculated using this algorithm for classic scoring: score_per_hitobject / max_per_hitobject * 936
        /// "936" is simplified from "75% * 4 * 300 * (1 + 1/25)"
        /// </remarks>
        [TestCase(ScoringMode.Standardised, HitResult.Miss, HitResult.Great, 0)]
        [TestCase(ScoringMode.Standardised, HitResult.Meh, HitResult.Great, 387_500)]
        [TestCase(ScoringMode.Standardised, HitResult.Ok, HitResult.Great, 425_000)]
        [TestCase(ScoringMode.Standardised, HitResult.Good, HitResult.Perfect, 478_571)]
        [TestCase(ScoringMode.Standardised, HitResult.Great, HitResult.Great, 575_000)]
        [TestCase(ScoringMode.Standardised, HitResult.Perfect, HitResult.Perfect, 575_000)]
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickMiss, HitResult.SmallTickHit, 700_000)]
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickHit, HitResult.SmallTickHit, 925_000)]
        [TestCase(ScoringMode.Standardised, HitResult.LargeTickMiss, HitResult.LargeTickHit, 0)]
        [TestCase(ScoringMode.Standardised, HitResult.LargeTickHit, HitResult.LargeTickHit, 575_000)]
        [TestCase(ScoringMode.Standardised, HitResult.SmallBonus, HitResult.SmallBonus, 700_030)]
        [TestCase(ScoringMode.Standardised, HitResult.LargeBonus, HitResult.LargeBonus, 700_150)]
        [TestCase(ScoringMode.Classic, HitResult.Miss, HitResult.Great, 0)]
        [TestCase(ScoringMode.Classic, HitResult.Meh, HitResult.Great, 156)]
        [TestCase(ScoringMode.Classic, HitResult.Ok, HitResult.Great, 312)]
        [TestCase(ScoringMode.Classic, HitResult.Good, HitResult.Perfect, 535)]
        [TestCase(ScoringMode.Classic, HitResult.Great, HitResult.Great, 936)]
        [TestCase(ScoringMode.Classic, HitResult.Perfect, HitResult.Perfect, 936)]
        [TestCase(ScoringMode.Classic, HitResult.SmallTickMiss, HitResult.SmallTickHit, 0)]
        [TestCase(ScoringMode.Classic, HitResult.SmallTickHit, HitResult.SmallTickHit, 225)]
        [TestCase(ScoringMode.Classic, HitResult.LargeTickMiss, HitResult.LargeTickHit, 0)]
        [TestCase(ScoringMode.Classic, HitResult.LargeTickHit, HitResult.LargeTickHit, 936)]
        [TestCase(ScoringMode.Classic, HitResult.SmallBonus, HitResult.SmallBonus, 30)]
        [TestCase(ScoringMode.Classic, HitResult.LargeBonus, HitResult.LargeBonus, 150)]
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

        [TestCase(ScoringMode.Standardised, HitResult.SmallTickHit, 978_571)]
        [TestCase(ScoringMode.Standardised, HitResult.SmallTickMiss, 914_286)]
        [TestCase(ScoringMode.Classic, HitResult.SmallTickHit, 279)]
        [TestCase(ScoringMode.Classic, HitResult.SmallTickMiss, 214)]
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
