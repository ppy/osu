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
            Assert.IsTrue(hitResult.AffectsCombo() == expectedReturnValue);
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
            Assert.IsTrue(hitResult.AffectsAccuracy() == expectedReturnValue);
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
            Assert.IsTrue(hitResult.IsBonus() == expectedReturnValue);
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
            Assert.IsTrue(hitResult.IsHit() == expectedReturnValue);
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
            Assert.IsTrue(hitResult.IsScorable() == expectedReturnValue);
        }
    }
}
