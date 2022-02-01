// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

            var scoreProcessor = new ScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a miss judgement
            scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new TestJudgement()) { Type = HitResult.Miss });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(0.0));
        }

        [Test]
        public void TestOnlyBonusScore()
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = { new HitObject() } };

            var scoreProcessor = new ScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a judgement
            scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new TestJudgement(HitResult.LargeBonus)) { Type = HitResult.LargeBonus });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(Judgement.LARGE_BONUS_SCORE));
        }

        [Test]
        public void TestResetFromReplayFrame()
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = { new HitCircle() } };

            var scoreProcessor = new ScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new TestJudgement(HitResult.Great)) { Type = HitResult.Great });
            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(1_000_000));
            Assert.That(scoreProcessor.JudgedHits, Is.EqualTo(1));

            // No header shouldn't cause any change
            scoreProcessor.ResetFromReplayFrame(new OsuRuleset(), new OsuReplayFrame());

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(1_000_000));
            Assert.That(scoreProcessor.JudgedHits, Is.EqualTo(1));

            // Reset with a miss instead.
            scoreProcessor.ResetFromReplayFrame(new OsuRuleset(), new OsuReplayFrame
            {
                Header = new FrameHeader(0, 0, 0, new Dictionary<HitResult, int> { { HitResult.Miss, 1 } }, DateTimeOffset.Now)
            });

            Assert.That(scoreProcessor.TotalScore.Value, Is.Zero);
            Assert.That(scoreProcessor.JudgedHits, Is.EqualTo(1));

            // Reset with no judged hit.
            scoreProcessor.ResetFromReplayFrame(new OsuRuleset(), new OsuReplayFrame
            {
                Header = new FrameHeader(0, 0, 0, new Dictionary<HitResult, int>(), DateTimeOffset.Now)
            });

            Assert.That(scoreProcessor.TotalScore.Value, Is.Zero);
            Assert.That(scoreProcessor.JudgedHits, Is.Zero);
        }

        private class TestJudgement : Judgement
        {
            public override HitResult MaxResult { get; }

            public TestJudgement(HitResult maxResult = HitResult.Perfect)
            {
                MaxResult = maxResult;
            }
        }
    }
}
