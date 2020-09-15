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

        [Test]
        public void TestOnlyBonusScore()
        {
            var beatmap = new Beatmap<TestHitObject> { HitObjects = { new TestHitObject(false) } };

            var scoreProcessor = new ScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            // Apply a judgement
            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], beatmap.HitObjects[0].CreateJudgement()) { Type = HitResult.Perfect });

            Assert.That(scoreProcessor.TotalScore.Value, Is.EqualTo(100));
        }

        private class TestHitObject : HitObject
        {
            private readonly bool affectsCombo;
            private readonly int numericResult;
            private readonly HitResult maxResult;

            public TestHitObject(bool affectsCombo = true, int numericResult = 100, HitResult maxResult = HitResult.Perfect)
            {
                this.affectsCombo = affectsCombo;
                this.numericResult = numericResult;
                this.maxResult = maxResult;
            }

            public override Judgement CreateJudgement() => new TestJudgement(affectsCombo, numericResult, maxResult);
        }

        private class TestJudgement : Judgement
        {
            private readonly int numericResult;

            public TestJudgement(bool affectsCombo = true, int numericResult = 100, HitResult maxResult = HitResult.Perfect)
            {
                AffectsCombo = affectsCombo;
                this.numericResult = numericResult;
                MaxResult = maxResult;
            }

            protected override int NumericResultFor(HitResult result) => numericResult;

            public override bool AffectsCombo { get; }

            public override HitResult MaxResult { get; }
        }
    }
}
