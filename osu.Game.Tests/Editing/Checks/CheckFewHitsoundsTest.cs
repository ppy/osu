// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckFewHitsoundsTest
    {
        private CheckFewHitsounds check = null!;

        private List<HitSampleInfo> notHitsounded = null!;
        private List<HitSampleInfo> hitsounded = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckFewHitsounds();
            notHitsounded = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) };
            hitsounded = new List<HitSampleInfo>
            {
                new HitSampleInfo(HitSampleInfo.HIT_NORMAL),
                new HitSampleInfo(HitSampleInfo.HIT_FINISH)
            };
        }

        [Test]
        public void TestHitsounded()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i < 16; ++i)
            {
                var samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) };

                if ((i + 1) % 2 == 0)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_CLAP));
                if ((i + 1) % 3 == 0)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE));
                if ((i + 1) % 4 == 0)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_FINISH));

                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = samples });
            }

            assertOk(hitObjects);
        }

        [Test]
        public void TestHitsoundedWithBreak()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i < 32; ++i)
            {
                var samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) };

                if ((i + 1) % 2 == 0)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_CLAP));
                if ((i + 1) % 3 == 0)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE));
                if ((i + 1) % 4 == 0)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_FINISH));
                // Leaves a gap in which no hitsounds exist or can be added, and so shouldn't be an issue.
                if (i > 8 && i < 24)
                    continue;

                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = samples });
            }

            assertOk(hitObjects);
        }

        [Test]
        public void TestLightlyHitsounded()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i < 30; ++i)
            {
                var samples = i % 8 == 0 ? hitsounded : notHitsounded;

                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = samples });
            }

            assertLongPeriodNegligible(hitObjects, count: 3);
        }

        [Test]
        public void TestRarelyHitsounded()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i < 30; ++i)
            {
                var samples = (i == 0 || i == 15) ? hitsounded : notHitsounded;

                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = samples });
            }

            // Should prompt one warning between 1st and 16th, and another between 16th and 31st.
            assertLongPeriodWarning(hitObjects, count: 2);
        }

        [Test]
        public void TestExtremelyRarelyHitsounded()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i < 80; ++i)
            {
                var samples = i == 40 ? hitsounded : notHitsounded;

                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = samples });
            }

            // Should prompt one problem between 1st and 41st, and another between 41st and 81st.
            assertLongPeriodProblem(hitObjects, count: 2);
        }

        [Test]
        public void TestNotHitsounded()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i < 20; ++i)
                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = notHitsounded });

            assertNoHitsounds(hitObjects);
        }

        [Test]
        public void TestNestedObjectsHitsounded()
        {
            var ticks = new List<HitObject>();
            for (int i = 1; i < 16; ++i)
                ticks.Add(new SliderTick { StartTime = 1000 * i, Samples = hitsounded });

            var nested = new MockNestableHitObject(ticks.ToList(), 0, 16000)
            {
                Samples = hitsounded
            };
            nested.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            assertOk(new List<HitObject> { nested });
        }

        [Test]
        public void TestNestedObjectsRarelyHitsounded()
        {
            var ticks = new List<HitObject>();
            for (int i = 1; i < 16; ++i)
                ticks.Add(new SliderTick { StartTime = 1000 * i, Samples = i == 0 ? hitsounded : notHitsounded });

            var nested = new MockNestableHitObject(ticks.ToList(), 0, 16000)
            {
                Samples = hitsounded
            };
            nested.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            assertLongPeriodWarning(new List<HitObject> { nested });
        }

        [Test]
        public void TestConcurrentObjects()
        {
            var hitObjects = new List<HitObject>();

            var ticks = new List<HitObject>();
            for (int i = 1; i < 10; ++i)
                ticks.Add(new SliderTick { StartTime = 5000 * i, Samples = hitsounded });

            var nested = new MockNestableHitObject(ticks.ToList(), 0, 50000)
            {
                Samples = notHitsounded
            };
            nested.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            hitObjects.Add(nested);

            for (int i = 1; i <= 6; ++i)
                hitObjects.Add(new HitCircle { StartTime = 10000 * i, Samples = notHitsounded });

            assertOk(hitObjects);
        }

        private void assertOk(List<HitObject> hitObjects)
        {
            Assert.That(check.Run(getContext(hitObjects)), Is.Empty);
        }

        private void assertLongPeriodProblem(List<HitObject> hitObjects, int count = 1)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckFewHitsounds.IssueTemplateLongPeriodProblem));
        }

        private void assertLongPeriodWarning(List<HitObject> hitObjects, int count = 1)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckFewHitsounds.IssueTemplateLongPeriodWarning));
        }

        private void assertLongPeriodNegligible(List<HitObject> hitObjects, int count = 1)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckFewHitsounds.IssueTemplateLongPeriodNegligible));
        }

        private void assertNoHitsounds(List<HitObject> hitObjects)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Any(issue => issue.Template is CheckFewHitsounds.IssueTemplateNoHitsounds));
        }

        private BeatmapVerifierContext getContext(List<HitObject> hitObjects)
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = hitObjects };

            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
