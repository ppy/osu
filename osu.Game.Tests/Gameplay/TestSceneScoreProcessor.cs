// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
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
            var beatmap = new Beatmap<TestHitObject> { HitObjects = { new TestHitObject() } };

            var scoreProcessor = new ScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a miss judgement
            scoreProcessor.ApplyResult(new JudgementResult(new TestHitObject(), new TestJudgement()) { Type = HitResult.Miss });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(0.0));
        }

        private class TestHitObject : HitObject
        {
            public override Judgement CreateJudgement() => new TestJudgement();
        }

        private class TestJudgement : Judgement
        {
            protected override int NumericResultFor(HitResult result) => 100;
        }
    }
}
