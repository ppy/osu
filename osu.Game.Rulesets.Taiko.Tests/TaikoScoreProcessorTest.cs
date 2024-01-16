// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TaikoScoreProcessorTest
    {
        [Test]
        public void TestInaccurateHitScore()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new Hit(),
                    new Hit { StartTime = 1000 }
                }
            };

            var scoreProcessor = new TaikoScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a miss judgement
            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[0], new TaikoJudgementCriteria()) { Type = HitResult.Great });
            scoreProcessor.ApplyResult(new Judgement(beatmap.HitObjects[1], new TaikoJudgementCriteria()) { Type = HitResult.Ok });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(453745));
            Assert.That(scoreProcessor.Accuracy.Value, Is.EqualTo(0.75).Within(0.0001));
        }
    }
}
