// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Mania.Edit.Checks;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckManiaConcurrentObjectsTest
    {
        private CheckConcurrentObjects check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckManiaConcurrentObjects();
        }

        [Test]
        public void TestHoldNotesSeparateOnSameColumn()
        {
            assertOk(new List<HitObject>
            {
                createHoldNote(startTime: 100, endTime: 400.75d, column: 1),
                createHoldNote(startTime: 500, endTime: 900.75d, column: 1)
            });
        }

        [Test]
        public void TestHoldNotesConcurrentOnDifferentColumns()
        {
            assertOk(new List<HitObject>
            {
                createHoldNote(startTime: 100, endTime: 400.75d, column: 1),
                createHoldNote(startTime: 300, endTime: 700.75d, column: 2)
            });
        }

        [Test]
        public void TestHoldNotesConcurrentOnSameColumn()
        {
            assertConcurrentSame(new List<HitObject>
            {
                createHoldNote(startTime: 100, endTime: 400.75d, column: 1),
                createHoldNote(startTime: 300, endTime: 700.75d, column: 1)
            });
        }

        private void assertOk(List<HitObject> hitobjects)
        {
            Assert.That(check.Run(getContext(hitobjects)), Is.Empty);
        }

        private void assertConcurrentSame(List<HitObject> hitobjects, int count = 1)
        {
            var issues = check.Run(getContext(hitobjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckConcurrentObjects.IssueTemplateConcurrentSame));
        }

        private BeatmapVerifierContext getContext(List<HitObject> hitobjects)
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = hitobjects };
            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }

        private HoldNote createHoldNote(double startTime, double endTime, int column)
        {
            return new HoldNote
            {
                StartTime = startTime,
                EndTime = endTime,
                Column = column
            };
        }
    }
}
