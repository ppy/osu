// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneDrainingHealthProcessor : OsuTestScene
    {
        private HealthProcessor processor;
        private ManualClock clock;

        [Test]
        public void TestInitialHealthStartsAtOne()
        {
            createProcessor(createBeatmap(1000, 2000));

            assertHealthEqualTo(1);
        }

        [Test]
        public void TestHealthNotDrainedBeforeGameplayStart()
        {
            createProcessor(createBeatmap(1000, 2000));

            setTime(100);
            assertHealthEqualTo(1);
            setTime(900);
            assertHealthEqualTo(1);
        }

        [Test]
        public void TestHealthDrainBetweenBreakAndObjects()
        {
            createProcessor(createBeatmap(0, 2000, new BreakPeriod(325, 375)));

            //               275    300    325    350    375    400    425
            // hitobjects            o                           o
            // break                        [-------------]
            // no drain              [---------------------------]

            setTime(285);
            setHealth(1);

            setTime(295);
            assertHealthNotEqualTo(1);

            setTime(305);
            setHealth(1);

            setTime(315);
            assertHealthEqualTo(1);

            setTime(365);
            assertHealthEqualTo(1);

            setTime(395);
            assertHealthEqualTo(1);

            setTime(425);
            assertHealthNotEqualTo(1);
        }

        [Test]
        public void TestHealthDrainDuringMaximalBreak()
        {
            createProcessor(createBeatmap(0, 2000, new BreakPeriod(300, 400)));

            //               275    300    325    350    375    400    425
            // hitobjects            o                           o
            // break                 [---------------------------]
            // no drain              [---------------------------]

            setTime(285);
            setHealth(1);

            setTime(295);
            assertHealthNotEqualTo(1);

            setTime(305);
            setHealth(1);

            setTime(395);
            assertHealthEqualTo(1);

            setTime(425);
            assertHealthNotEqualTo(1);
        }

        [Test]
        public void TestHealthNotDrainedAfterGameplayEnd()
        {
            createProcessor(createBeatmap(1000, 2000));
            setTime(2001); // After the hitobjects
            setHealth(1); // Reset the current health for assertions to take place

            setTime(2100);
            assertHealthEqualTo(1);
            setTime(3000);
            assertHealthEqualTo(1);
        }

        [Test]
        public void TestHealthDrainedDuringGameplay()
        {
            createProcessor(createBeatmap(0, 1000));

            setTime(500);
            assertHealthNotEqualTo(1);
        }

        [Test]
        public void TestHealthGainedAfterRewind()
        {
            createProcessor(createBeatmap(0, 1000));
            setTime(500);

            setTime(0);
            assertHealthEqualTo(1);
        }

        [Test]
        public void TestHealthGainedOnHit()
        {
            Beatmap beatmap = createBeatmap(0, 1000);

            createProcessor(beatmap);
            setTime(10); // Decrease health slightly
            assertHealthNotEqualTo(1);

            AddStep("apply hit result", () => processor.ApplyResult(new Judgement(beatmap.HitObjects[0], new JudgementCriteria()) { Type = HitResult.Perfect }));
            assertHealthEqualTo(1);
        }

        [Test]
        public void TestHealthRemovedOnRevert()
        {
            var beatmap = createBeatmap(0, 1000);
            Judgement result = null;

            createProcessor(beatmap);
            setTime(10); // Decrease health slightly
            AddStep("apply hit result", () => processor.ApplyResult(result = new Judgement(beatmap.HitObjects[0], new JudgementCriteria()) { Type = HitResult.Perfect }));

            AddStep("revert hit result", () => processor.RevertResult(result));
            assertHealthNotEqualTo(1);
        }

        [Test]
        public void TestFailConditions()
        {
            var beatmap = createBeatmap(0, 1000);
            createProcessor(beatmap);

            AddStep("setup fail conditions", () => processor.FailConditions += ((_, result) => result.Type == HitResult.Miss));

            AddStep("apply perfect hit result", () => processor.ApplyResult(new Judgement(beatmap.HitObjects[0], new JudgementCriteria()) { Type = HitResult.Perfect }));
            AddAssert("not failed", () => !processor.HasFailed);
            AddStep("apply miss hit result", () => processor.ApplyResult(new Judgement(beatmap.HitObjects[0], new JudgementCriteria()) { Type = HitResult.Miss }));
            AddAssert("failed", () => processor.HasFailed);
        }

        [TestCase(HitResult.Miss)]
        [TestCase(HitResult.Meh)]
        public void TestMultipleFailConditions(HitResult resultApplied)
        {
            var beatmap = createBeatmap(0, 1000);
            createProcessor(beatmap);

            AddStep("setup multiple fail conditions", () =>
            {
                processor.FailConditions += ((_, result) => result.Type == HitResult.Miss);
                processor.FailConditions += ((_, result) => result.Type == HitResult.Meh);
            });

            AddStep("apply perfect hit result", () => processor.ApplyResult(new Judgement(beatmap.HitObjects[0], new JudgementCriteria()) { Type = HitResult.Perfect }));
            AddAssert("not failed", () => !processor.HasFailed);

            AddStep($"apply {resultApplied.ToString().ToLowerInvariant()} hit result",
                () => processor.ApplyResult(new Judgement(beatmap.HitObjects[0], new JudgementCriteria()) { Type = resultApplied }));
            AddAssert("failed", () => processor.HasFailed);
        }

        [Test]
        public void TestBonusObjectsExcludedFromDrain()
        {
            var beatmap = new Beatmap
            {
                Difficulty = { DrainRate = 10 }
            };

            beatmap.HitObjects.Add(new JudgeableHitObject { StartTime = 0 });
            for (double time = 0; time < 5000; time += 100)
                beatmap.HitObjects.Add(new JudgeableHitObject(HitResult.LargeBonus) { StartTime = time });
            beatmap.HitObjects.Add(new JudgeableHitObject { StartTime = 5000 });

            createProcessor(beatmap);
            setTime(4900); // Get close to the second combo-affecting object
            assertHealthNotEqualTo(0);
        }

        [Test]
        public void TestSingleLongObjectDoesNotDrain()
        {
            var beatmap = new Beatmap
            {
                HitObjects = { new JudgeableLongHitObject() }
            };

            beatmap.HitObjects[0].ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            createProcessor(beatmap);
            setTime(0);
            assertHealthEqualTo(1);

            setTime(5000);
            assertHealthEqualTo(1);
        }

        [Test]
        public void TestNoBreakDrainRate()
        {
            DrainingHealthProcessor hp = new DrainingHealthProcessor(-1000);
            hp.ApplyBeatmap(new Beatmap<JudgeableHitObject>
            {
                HitObjects =
                {
                    new JudgeableHitObject { StartTime = 0 },
                    new JudgeableHitObject { StartTime = 2000 }
                }
            });

            Assert.That(hp.DrainRate, Is.EqualTo(4.5E-5).Within(0.1E-5));
        }

        [Test]
        public void TestSingleBreakDrainRate()
        {
            DrainingHealthProcessor hp = new DrainingHealthProcessor(-1000);
            hp.ApplyBeatmap(new Beatmap<JudgeableHitObject>
            {
                HitObjects =
                {
                    new JudgeableHitObject { StartTime = 0 },
                    new JudgeableHitObject { StartTime = 2000 }
                },
                Breaks =
                {
                    new BreakPeriod(500, 1500)
                }
            });

            Assert.That(hp.DrainRate, Is.EqualTo(9.1E-5).Within(0.1E-5));
        }

        [Test]
        public void TestOverlappingBreakDrainRate()
        {
            DrainingHealthProcessor hp = new DrainingHealthProcessor(-1000);
            hp.ApplyBeatmap(new Beatmap<JudgeableHitObject>
            {
                HitObjects =
                {
                    new JudgeableHitObject { StartTime = 0 },
                    new JudgeableHitObject { StartTime = 2000 }
                },
                Breaks =
                {
                    new BreakPeriod(500, 1400),
                    new BreakPeriod(750, 1500),
                }
            });

            Assert.That(hp.DrainRate, Is.EqualTo(9.1E-5).Within(0.1E-5));
        }

        [Test]
        public void TestSequentialBreakDrainRate()
        {
            DrainingHealthProcessor hp = new DrainingHealthProcessor(-1000);
            hp.ApplyBeatmap(new Beatmap<JudgeableHitObject>
            {
                HitObjects =
                {
                    new JudgeableHitObject { StartTime = 0 },
                    new JudgeableHitObject { StartTime = 2000 }
                },
                Breaks =
                {
                    new BreakPeriod(500, 1000),
                    new BreakPeriod(1000, 1500),
                }
            });

            Assert.That(hp.DrainRate, Is.EqualTo(9.1E-5).Within(0.1E-5));
        }

        private Beatmap createBeatmap(double startTime, double endTime, params BreakPeriod[] breaks)
        {
            var beatmap = new Beatmap
            {
                Difficulty = { DrainRate = 10 }
            };

            for (double time = startTime; time <= endTime; time += 100)
            {
                beatmap.HitObjects.Add(new JudgeableHitObject { StartTime = time });
            }

            beatmap.Breaks.AddRange(breaks);

            return beatmap;
        }

        private void createProcessor(Beatmap beatmap) => AddStep("create processor", () =>
        {
            Child = processor = new DrainingHealthProcessor(beatmap.HitObjects[0].StartTime).With(d =>
            {
                d.RelativeSizeAxes = Axes.Both;
                d.Clock = new FramedClock(clock = new ManualClock());
            });

            processor.ApplyBeatmap(beatmap);
        });

        private void setTime(double time) => AddStep($"set time = {time}", () => clock.CurrentTime = time);

        private void setHealth(double health) => AddStep($"set health = {health}", () => processor.Health.Value = health);

        private void assertHealthEqualTo(double value)
            => AddAssert($"health = {value}", () => Precision.AlmostEquals(value, processor.Health.Value, 0.0001f));

        private void assertHealthNotEqualTo(double value)
            => AddAssert($"health != {value}", () => !Precision.AlmostEquals(value, processor.Health.Value, 0.0001f));

        private class JudgeableHitObject : HitObject
        {
            private readonly HitResult maxResult;

            public JudgeableHitObject(HitResult maxResult = HitResult.Perfect)
            {
                this.maxResult = maxResult;
            }

            public override JudgementCriteria CreateJudgement() => new TestJudgementCriteria(maxResult);
            protected override HitWindows CreateHitWindows() => new HitWindows();

            private class TestJudgementCriteria : JudgementCriteria
            {
                public override HitResult MaxResult { get; }

                public TestJudgementCriteria(HitResult maxResult)
                {
                    MaxResult = maxResult;
                }
            }
        }

        private class JudgeableLongHitObject : JudgeableHitObject, IHasDuration
        {
            public double EndTime => StartTime + Duration;
            public double Duration { get; set; } = 5000;

            public JudgeableLongHitObject()
                : base(HitResult.LargeBonus)
            {
            }

            protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
            {
                base.CreateNestedHitObjects(cancellationToken);

                AddNested(new JudgeableHitObject());
            }
        }
    }
}
