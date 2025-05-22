// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [HeadlessTest]
    public partial class TestSceneOsuScoreProcessor : OsuTestScene
    {
        [Test]
        public void TestMinimumRankShouldNeverReachSRankBeforeComplete()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = Enumerable.Range(0, 1000).Select(_ => new TestHitObject(HitResult.Great)).ToList<HitObject>()
            };

            var scoreProcessor = new OsuScoreProcessor();
            scoreProcessor.ApplyBeatmap(beatmap);

            for (int i = 0; i < 999; i++)
                scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[i], beatmap.HitObjects[i].Judgement) { Type = HitResult.Great });

            Assert.That(scoreProcessor.Accuracy.Value, Is.EqualTo(1));
            Assert.That(scoreProcessor.MinimumAccuracy.Value, Is.EqualTo(0.99).Within(0.01));
            Assert.That(scoreProcessor.MaximumAccuracy.Value, Is.EqualTo(1));

            Assert.That(scoreProcessor.Rank.Value, Is.EqualTo(ScoreRank.X));
            Assert.That(scoreProcessor.MinimumRank.Value, Is.EqualTo(ScoreRank.A));
            Assert.That(scoreProcessor.MaximumRank.Value, Is.EqualTo(ScoreRank.X));

            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[999], beatmap.HitObjects[999].Judgement) { Type = HitResult.Great });

            Assert.That(scoreProcessor.Rank.Value, Is.EqualTo(ScoreRank.X));
            Assert.That(scoreProcessor.MinimumRank.Value, Is.EqualTo(ScoreRank.X));
            Assert.That(scoreProcessor.MaximumRank.Value, Is.EqualTo(ScoreRank.X));
        }

        [Test]
        public void TestMinimumAndMaximumRankAdjustedByMods()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = Enumerable.Range(0, 1000).Select(_ => new TestHitObject(HitResult.Great)).ToList<HitObject>()
            };

            var scoreProcessor = new OsuScoreProcessor();
            scoreProcessor.Mods.Value = new[] { new OsuModHidden() };
            scoreProcessor.ApplyBeatmap(beatmap);

            for (int i = 0; i < 999; i++)
                scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[i], beatmap.HitObjects[i].Judgement) { Type = HitResult.Great });

            Assert.That(scoreProcessor.Rank.Value, Is.EqualTo(ScoreRank.XH));
            Assert.That(scoreProcessor.MinimumRank.Value, Is.EqualTo(ScoreRank.A));
            Assert.That(scoreProcessor.MaximumRank.Value, Is.EqualTo(ScoreRank.XH));

            scoreProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[999], beatmap.HitObjects[999].Judgement) { Type = HitResult.Great });

            Assert.That(scoreProcessor.Rank.Value, Is.EqualTo(ScoreRank.XH));
            Assert.That(scoreProcessor.MinimumRank.Value, Is.EqualTo(ScoreRank.XH));
            Assert.That(scoreProcessor.MaximumRank.Value, Is.EqualTo(ScoreRank.XH));
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
