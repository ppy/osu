// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    public class CheckDrainTimeTest
    {
        private CheckDrainTime check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckDrainTime();
        }

        [Test]
        public void TestDrainTimeShort()
        {
            assertShortDrainTime(getShortDrainTimeBeatmap());
        }

        [Test]
        public void TestDrainTimeBreak()
        {
            assertShortDrainTime(getLongBreakBeatmap());
        }

        [Test]
        public void TestDrainTimeCorrect()
        {
            assertOk(getCorrectDrainTimeBeatmap());
        }

        private IBeatmap getShortDrainTimeBeatmap()
        {
            return new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 }
                }
            };
        }

        private IBeatmap getLongBreakBeatmap()
        {
            return new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 30 }
                }
            };
        }

        private IBeatmap getCorrectDrainTimeBeatmap()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i <= 30; ++i)
            {
                hitObjects.Add(new HitCircle { StartTime = 1000 * i });
            }

            return new Beatmap<HitObject>
            {
                HitObjects = hitObjects
            };
        }

        private void assertShortDrainTime(IBeatmap beatmap)
        {
            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckDrainTime.IssueTemplateTooShort);
        }

        private void assertOk(IBeatmap beatmap)
        {
            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Is.Empty);
        }

        private BeatmapVerifierContext getContext(IBeatmap beatmap)
        {
            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
