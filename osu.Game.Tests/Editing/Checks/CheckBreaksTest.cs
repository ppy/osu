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
    public class CheckBreaksTest
    {
        private CheckBreaks check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckBreaks();
        }

        [Test]
        public void TestBreakTooShort()
        {
            var beatmap = new Beatmap<HitObject>
            {
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(0, 649)
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBreaks.IssueTemplateTooShort);
        }

        [Test]
        public void TestBreakStartsEarly()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 1_200 }
                },
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(100, 751)
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBreaks.IssueTemplateEarlyStart);
        }

        [Test]
        public void TestBreakEndsLate()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 1_298 }
                },
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(200, 850)
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBreaks.IssueTemplateLateEnd);
        }

        [Test]
        public void TestBreakAfterLastObjectStartsEarly()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 1200 }
                },
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(1398, 2300)
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBreaks.IssueTemplateEarlyStart);
        }

        [Test]
        public void TestBreakBeforeFirstObjectEndsLate()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 1100 },
                    new HitCircle { StartTime = 1500 }
                },
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(0, 652)
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBreaks.IssueTemplateLateEnd);
        }

        [Test]
        public void TestBreakMultipleObjectsEarly()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 1_297 },
                    new HitCircle { StartTime = 1_298 }
                },
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(200, 850)
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBreaks.IssueTemplateLateEnd);
        }

        [Test]
        public void TestBreaksCorrect()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 1_300 }
                },
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(200, 850)
                }
            };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Is.Empty);
        }
    }
}
