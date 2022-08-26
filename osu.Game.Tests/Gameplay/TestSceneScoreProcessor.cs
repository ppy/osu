// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
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
    public class TestSceneScoreProcessor : OsuTestScene
    {
        [Test]
        public void TestNoScoreIncreaseFromMiss()
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = { new HitObject() } };

            var scoreProcessor = new ScoreProcessor(new OsuRuleset());
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a miss judgement
            scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new TestJudgement()) { Type = HitResult.Miss });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(0.0));
        }

        [Test]
        public void TestOnlyBonusScore()
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = { new HitObject() } };

            var scoreProcessor = new ScoreProcessor(new OsuRuleset());
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a judgement
            scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new TestJudgement(HitResult.LargeBonus)) { Type = HitResult.LargeBonus });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(Judgement.LARGE_BONUS_SCORE));
        }

        [Test]
        public void TestResetFromReplayFrame()
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = { new HitCircle() } };

            var scoreProcessor = new ScoreProcessor(new OsuRuleset());
            scoreProcessor.ApplyBeatmap(beatmap);

            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new TestJudgement(HitResult.Great)) { Type = HitResult.Great });
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
                Header = new FrameHeader(0, 0, 0, new Dictionary<HitResult, int> { { HitResult.Miss, 1 } }, DateTimeOffset.Now)
            });

            Assert.That(scoreProcessor.TotalScore.Value, Is.Zero);
            Assert.That(scoreProcessor.JudgedHits, Is.EqualTo(1));
            Assert.That(scoreProcessor.Combo.Value, Is.EqualTo(0));

            // Reset with no judged hit.
            scoreProcessor.ResetFromReplayFrame(new OsuReplayFrame
            {
                Header = new FrameHeader(0, 0, 0, new Dictionary<HitResult, int>(), DateTimeOffset.Now)
            });

            Assert.That(scoreProcessor.TotalScore.Value, Is.Zero);
            Assert.That(scoreProcessor.JudgedHits, Is.Zero);
            Assert.That(scoreProcessor.Combo.Value, Is.EqualTo(0));
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

            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], beatmap.HitObjects[0].CreateJudgement()) { Type = HitResult.Ok });
            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], beatmap.HitObjects[1].CreateJudgement()) { Type = HitResult.LargeTickHit });
            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[2], beatmap.HitObjects[2].CreateJudgement()) { Type = HitResult.SmallTickMiss });
            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[3], beatmap.HitObjects[3].CreateJudgement()) { Type = HitResult.SmallBonus });

            var score = new ScoreInfo { Ruleset = new OsuRuleset().RulesetInfo };
            scoreProcessor.FailScore(score);

            Assert.That(score.Rank, Is.EqualTo(ScoreRank.F));
            Assert.That(score.Passed, Is.False);
            Assert.That(score.Statistics.Count(kvp => kvp.Value > 0), Is.EqualTo(7));
            Assert.That(score.Statistics[HitResult.Ok], Is.EqualTo(1));
            Assert.That(score.Statistics[HitResult.Miss], Is.EqualTo(1));
            Assert.That(score.Statistics[HitResult.LargeTickHit], Is.EqualTo(1));
            Assert.That(score.Statistics[HitResult.LargeTickMiss], Is.EqualTo(1));
            Assert.That(score.Statistics[HitResult.SmallTickMiss], Is.EqualTo(2));
            Assert.That(score.Statistics[HitResult.SmallBonus], Is.EqualTo(1));
            Assert.That(score.Statistics[HitResult.IgnoreMiss], Is.EqualTo(1));
        }

        private class TestJudgement : Judgement
        {
            public override HitResult MaxResult { get; }

            public TestJudgement(HitResult maxResult = HitResult.Perfect)
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

            public override Judgement CreateJudgement() => new TestJudgement(maxResult);
        }
    }
}
