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

        [Test]
        public void TestDrumRollOnly()
        {
            var beatmap = new TaikoBeatmap
            {
                HitObjects =
                {
                    new DrumRoll { Duration = 2000 }
                }
            };

            foreach (var ho in beatmap.HitObjects)
                ho.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            var healthProcessor = new TaikoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            foreach (var nested in beatmap.HitObjects[0].NestedHitObjects)
            {
                var nestedJudgement = nested.Judgement;
                healthProcessor.ApplyResult(new JudgementResult(nested, nestedJudgement) { Type = nestedJudgement.MaxResult });
            }

            var judgement = beatmap.HitObjects[0].Judgement;
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], judgement) { Type = judgement.MaxResult });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.EqualTo(1));
                Assert.That(healthProcessor.HasFailed, Is.False);
            });
        }

        [Test]
        public void TestSwellOnly()
        {
            var beatmap = new TaikoBeatmap
            {
                HitObjects =
                {
                    new Swell { Duration = 2000 }
                }
            };

            foreach (var ho in beatmap.HitObjects)
                ho.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            var healthProcessor = new TaikoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            foreach (var nested in beatmap.HitObjects[0].NestedHitObjects)
            {
                var nestedJudgement = nested.Judgement;
                healthProcessor.ApplyResult(new JudgementResult(nested, nestedJudgement) { Type = nestedJudgement.MaxResult });
            }

            var judgement = beatmap.HitObjects[0].Judgement;
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], judgement) { Type = judgement.MaxResult });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.EqualTo(1));
                Assert.That(healthProcessor.HasFailed, Is.False);
            });
        }

        [Test]
        public void TestMissHitAndHitSwell()
        {
            var beatmap = new TaikoBeatmap
            {
                HitObjects =
                {
                    new Hit(),
                    new Swell { Duration = 2000 }
                }
            };

            foreach (var ho in beatmap.HitObjects)
                ho.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            var healthProcessor = new TaikoHealthProcessor();
            healthProcessor.ApplyBeatmap(beatmap);

            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[0], new TaikoJudgement()) { Type = HitResult.Miss });

            foreach (var nested in beatmap.HitObjects[1].NestedHitObjects)
            {
                var nestedJudgement = nested.CreateJudgement();
                healthProcessor.ApplyResult(new JudgementResult(nested, nestedJudgement) { Type = nestedJudgement.MaxResult });
            }

            var judgement = beatmap.HitObjects[1].CreateJudgement();
            healthProcessor.ApplyResult(new JudgementResult(beatmap.HitObjects[1], judgement) { Type = judgement.MaxResult });

            Assert.Multiple(() =>
            {
                Assert.That(healthProcessor.Health.Value, Is.EqualTo(0));
                Assert.That(healthProcessor.HasFailed, Is.True);
            });
        }

        private static readonly object[][] test_cases =
        [
            // hitobject, fail expected after miss
            [new Hit(), true],
            [new Hit.StrongNestedHit(new Hit()), false],
            [new DrumRollTick(new DrumRoll()), false],
            [new DrumRollTick.StrongNestedHit(new DrumRollTick(new DrumRoll())), false],
            [new DrumRoll(), false],
            [new SwellTick(), false],
            [new Swell(), false]
        ];

        [TestCaseSource(nameof(test_cases))]
        public void TestFailAfterMinResult(TaikoHitObject hitObject, bool failExpected)
        {
            var healthProcessor = new TaikoHealthProcessor();
            healthProcessor.ApplyBeatmap(new TaikoBeatmap
            {
                HitObjects = { hitObject }
            });

            var result = new JudgementResult(hitObject, hitObject.CreateJudgement());
            result.Type = result.Judgement.MinResult;
            healthProcessor.ApplyResult(result);

            Assert.That(healthProcessor.HasFailed, Is.EqualTo(failExpected));
        }

        [TestCaseSource(nameof(test_cases))]
        public void TestNoFailAfterMaxResult(TaikoHitObject hitObject, bool _)
        {
            var healthProcessor = new TaikoHealthProcessor();
            healthProcessor.ApplyBeatmap(new TaikoBeatmap
            {
                HitObjects = { hitObject }
            });

            var result = new JudgementResult(hitObject, hitObject.CreateJudgement());
            result.Type = result.Judgement.MaxResult;
            healthProcessor.ApplyResult(result);

            Assert.That(healthProcessor.HasFailed, Is.False);
        }
    }
}
