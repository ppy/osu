// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneScoreProcessor : OsuTestScene
    {
        [Test]
        public void TestNoScoreIncreaseFromMiss()
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = { new HitObject() } };

            var scoreProcessor = new ScoreProcessor(new OsuRuleset());
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a miss judgement
            scoreProcessor.ApplyResult(new Judgement(new HitObject(), new TestJudgementInfo()) { Type = HitResult.Miss });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(0));
        }

        [Test]
        public void TestOnlyBonusScore()
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = { new HitObject() } };

            var scoreProcessor = new ScoreProcessor(new OsuRuleset());
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a judgement
            scoreProcessor.ApplyResult(new Judgement(new HitObject(), new TestJudgementInfo(HitResult.LargeBonus)) { Type = HitResult.LargeBonus });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(scoreProcessor.GetBaseScoreForResult(HitResult.LargeBonus)));
        }

        [Test]
        public void TestResetFromReplayFrame()
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = { new HitCircle() } };

            var scoreProcessor = new ScoreProcessor(new OsuRuleset());
            scoreProcessor.ApplyBeatmap(beatmap);

            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[0], new TestJudgementInfo(HitResult.Great)) { Type = HitResult.Great });
            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(1_000_000));
            Assert.That(scoreProcessor.JudgedHits, Is.EqualTo(1));
            Assert.That(scoreProcessor.Combo.Value, Is.EqualTo(1));

            // No header shouldn't cause any change
            scoreProcessor.ResetFromReplayFrame(new OsuReplayFrame());

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(1_000_000));
            Assert.That(scoreProcessor.JudgedHits, Is.EqualTo(1));
            Assert.That(scoreProcessor.Combo.Value, Is.EqualTo(1));

            // Reset with a miss instead.
            scoreProcessor.ResetFromReplayFrame(new OsuReplayFrame
            {
                Header = new FrameHeader(0, 0, 0, 0, new Dictionary<HitResult, int> { { HitResult.Miss, 1 } }, new ScoreProcessorStatistics
                {
                    MaximumBaseScore = 300,
                    BaseScore = 0,
                    AccuracyJudgementCount = 1,
                    ComboPortion = 0,
                    BonusPortion = 0
                }, DateTimeOffset.Now)
            });

            Assert.That(scoreProcessor.TotalScore.Value, Is.Zero);
            Assert.That(scoreProcessor.JudgedHits, Is.EqualTo(1));
            Assert.That(scoreProcessor.Combo.Value, Is.EqualTo(0));
            Assert.That(scoreProcessor.Accuracy.Value, Is.EqualTo(0));

            // Reset with no judged hit.
            scoreProcessor.ResetFromReplayFrame(new OsuReplayFrame
            {
                Header = new FrameHeader(0, 0, 0, 0, new Dictionary<HitResult, int>(), new ScoreProcessorStatistics
                {
                    MaximumBaseScore = 0,
                    BaseScore = 0,
                    AccuracyJudgementCount = 0,
                    ComboPortion = 0,
                    BonusPortion = 0
                }, DateTimeOffset.Now)
            });

            Assert.That(scoreProcessor.TotalScore.Value, Is.Zero);
            Assert.That(scoreProcessor.JudgedHits, Is.Zero);
            Assert.That(scoreProcessor.Combo.Value, Is.EqualTo(0));
            Assert.That(scoreProcessor.Accuracy.Value, Is.EqualTo(1));
        }

        [Test]
        public void TestFailScore()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new TestHitObject(),
                    new TestHitObject(HitResult.LargeTickHit),
                    new TestHitObject(HitResult.SmallTickHit),
                    new TestHitObject(HitResult.SmallBonus),
                    new TestHitObject(),
                    new TestHitObject(HitResult.LargeTickHit),
                    new TestHitObject(HitResult.SmallTickHit),
                    new TestHitObject(HitResult.LargeBonus),
                }
            };

            var scoreProcessor = new ScoreProcessor(new OsuRuleset());
            scoreProcessor.ApplyBeatmap(beatmap);

            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[0], beatmap.HitObjects[0].CreateJudgement()) { Type = HitResult.Ok });
            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[1], beatmap.HitObjects[1].CreateJudgement()) { Type = HitResult.LargeTickHit });
            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[2], beatmap.HitObjects[2].CreateJudgement()) { Type = HitResult.SmallTickMiss });
            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[3], beatmap.HitObjects[3].CreateJudgement()) { Type = HitResult.SmallBonus });

            var score = new ScoreInfo { Ruleset = new OsuRuleset().RulesetInfo };
            scoreProcessor.FailScore(score);

            Assert.That(score.Rank, Is.EqualTo(ScoreRank.F));
            Assert.That(score.Passed, Is.False);
            Assert.That(score.Statistics.Sum(kvp => kvp.Value), Is.EqualTo(4));
            Assert.That(score.MaximumStatistics.Sum(kvp => kvp.Value), Is.EqualTo(8));

            Assert.That(score.Statistics[HitResult.Ok], Is.EqualTo(1));
            Assert.That(score.Statistics[HitResult.LargeTickHit], Is.EqualTo(1));
            Assert.That(score.Statistics[HitResult.SmallTickMiss], Is.EqualTo(1));
            Assert.That(score.Statistics[HitResult.SmallBonus], Is.EqualTo(1));

            Assert.That(score.MaximumStatistics[HitResult.Perfect], Is.EqualTo(2));
            Assert.That(score.MaximumStatistics[HitResult.LargeTickHit], Is.EqualTo(2));
            Assert.That(score.MaximumStatistics[HitResult.SmallTickHit], Is.EqualTo(2));
            Assert.That(score.MaximumStatistics[HitResult.SmallBonus], Is.EqualTo(1));
            Assert.That(score.MaximumStatistics[HitResult.LargeBonus], Is.EqualTo(1));
        }

        [Test]
        public void TestAccuracyModes()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = Enumerable.Range(0, 4).Select(_ => new TestHitObject(HitResult.Great)).ToList<HitObject>()
            };

            var scoreProcessor = new ScoreProcessor(new OsuRuleset());
            scoreProcessor.ApplyBeatmap(beatmap);

            Assert.That(scoreProcessor.Accuracy.Value, Is.EqualTo(1));
            Assert.That(scoreProcessor.MinimumAccuracy.Value, Is.EqualTo(0));
            Assert.That(scoreProcessor.MaximumAccuracy.Value, Is.EqualTo(1));

            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[0], beatmap.HitObjects[0].CreateJudgement()) { Type = HitResult.Ok });
            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[1], beatmap.HitObjects[1].CreateJudgement()) { Type = HitResult.Great });

            Assert.That(scoreProcessor.Accuracy.Value, Is.EqualTo((double)(100 + 300) / (2 * 300)).Within(Precision.DOUBLE_EPSILON));
            Assert.That(scoreProcessor.MinimumAccuracy.Value, Is.EqualTo((double)(100 + 300) / (4 * 300)).Within(Precision.DOUBLE_EPSILON));
            Assert.That(scoreProcessor.MaximumAccuracy.Value, Is.EqualTo((double)(100 + 3 * 300) / (4 * 300)).Within(Precision.DOUBLE_EPSILON));
        }

        private class TestJudgementInfo : JudgementInfo
        {
            public override HitResult MaxResult { get; }

            public TestJudgementInfo(HitResult maxResult = HitResult.Perfect)
            {
                MaxResult = maxResult;
            }
        }

        private class TestHitObject : HitObject
        {
            private readonly HitResult maxResult;

            public TestHitObject(HitResult maxResult = HitResult.Perfect)
            {
                this.maxResult = maxResult;
            }

            public override JudgementInfo CreateJudgement() => new TestJudgementInfo(maxResult);
        }
    }
}
