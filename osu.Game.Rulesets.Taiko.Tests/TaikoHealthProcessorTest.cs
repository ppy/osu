// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TaikoHealthProcessorTest
    {
        [Test]
        public void TestHitsOnlyGreat()
        {
            var beatmap = new TaikoBeatmap
            {
                HitObjects =
                {
                    new Hit(),
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 },
                    new Hit { StartTime = 3000 },
                    new Hit { StartTime = 4000 },
                }
            };

            var healthProcessor = new TaikoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new TaikoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], new TaikoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[2], new TaikoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[3], new TaikoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[4], new TaikoJudgement()) { Type = HitResult.Great });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.EqualTo(1));
                Assert.That(healthProcessor.HasFailed, Is.False);
            });
        }

        [Test]
        public void TestHitsAboveThreshold()
        {
            var beatmap = new TaikoBeatmap
            {
                HitObjects =
                {
                    new Hit(),
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 },
                    new Hit { StartTime = 3000 },
                    new Hit { StartTime = 4000 },
                }
            };

            var healthProcessor = new TaikoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new TaikoJudgement()) { Type = HitResult.Great });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], new TaikoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[2], new TaikoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[3], new TaikoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[4], new TaikoJudgement()) { Type = HitResult.Miss });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.GreaterThan(0.5));
                Assert.That(healthProcessor.HasFailed, Is.False);
            });
        }

        [Test]
        public void TestHitsBelowThreshold()
        {
            var beatmap = new TaikoBeatmap
            {
                HitObjects =
                {
                    new Hit(),
                    new Hit { StartTime = 1000 },
                    new Hit { StartTime = 2000 },
                    new Hit { StartTime = 3000 },
                    new Hit { StartTime = 4000 },
                }
            };

            var healthProcessor = new TaikoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new TaikoJudgement()) { Type = HitResult.Miss });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], new TaikoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[2], new TaikoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[3], new TaikoJudgement()) { Type = HitResult.Ok });
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[4], new TaikoJudgement()) { Type = HitResult.Miss });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.LessThan(0.5));
                Assert.That(healthProcessor.HasFailed, Is.True);
            });
        }
    }
}
