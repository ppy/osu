// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    public class CheckDrainLengthTest
    {
        private CheckDrainLength check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckDrainLength();
        }

        [Test]
        public void TestDrainTimeShort()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 29_999 }
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckDrainLength.IssueTemplateTooShort);
        }

        [Test]
        public void TestDrainTimeBreak()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 40_000 }
                },
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(10_000, 21_000)
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckDrainLength.IssueTemplateTooShort);
        }

        [Test]
        public void TestDrainTimeCorrect()
        {
            var hitObjects = new List<HitObject>();

            for (int i = 0; i <= 30; ++i)
                hitObjects.Add(new HitCircle { StartTime = 1000 * i });

            var beatmap = new Beatmap<HitObject> { HitObjects = hitObjects };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Is.Empty);
        }
    }
}
