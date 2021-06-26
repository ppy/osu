// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
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
        private CheckFewHitsounds check;

        [SetUp]
        public void Setup()
        {
            check = new CheckFewHitsounds();
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
                var samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) };

                if (i % 8 == 0)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE));

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
                var samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) };

                if (i == 0 || i == 15)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE));

                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = samples });
            }

            // Should prompt one warning between 1st and 11th, and another between 11th and 20th.
            assertLongPeriodWarning(hitObjects, count: 2);
        }

        [Test]
        public void TestExtremelyRarelyHitsounded()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i < 80; ++i)
            {
                var samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) };

                if (i == 40)
                    samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE));

                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = samples });
            }

            // Should prompt one problem between 1st and 40th, and another between 40th and 80th.
            assertLongPeriodProblem(hitObjects, count: 2);
        }

        [Test]
        public void TestNotHitsounded()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i < 20; ++i)
            {
                var samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) };

                hitObjects.Add(new HitCircle { StartTime = 1000 * i, Samples = samples });
            }

            // Should prompt one problem between 1st and 40th, and another between 40th and 80th.
            assertNoHitsounds(hitObjects);
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
