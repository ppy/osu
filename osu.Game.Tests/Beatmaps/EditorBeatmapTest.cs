// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public class EditorBeatmapTest
    {
        /// <summary>
        /// Tests that the addition event is correctly invoked after a hitobject is added.
        /// </summary>
        [Test]
        public void TestHitObjectAddEvent()
        {
            var editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap());

            HitObject addedObject = null;
            editorBeatmap.HitObjectAdded += h => addedObject = h;

            var hitCircle = new HitCircle();

            editorBeatmap.Add(hitCircle);
            Assert.That(addedObject, Is.EqualTo(hitCircle));
        }

        /// <summary>
        /// Tests that the removal event is correctly invoked after a hitobject is removed.
        /// </summary>
        [Test]
        public void HitObjectRemoveEvent()
        {
            var hitCircle = new HitCircle();
            var editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap { HitObjects = { hitCircle } });

            HitObject removedObject = null;
            editorBeatmap.HitObjectRemoved += h => removedObject = h;

            editorBeatmap.Remove(hitCircle);
            Assert.That(removedObject, Is.EqualTo(hitCircle));
        }

        /// <summary>
        /// Tests that the changed event is correctly invoked after the start time of a hitobject is changed.
        /// This tests for hitobjects which were already present before the editor beatmap was constructed.
        /// </summary>
        [Test]
        public void TestInitialHitObjectStartTimeChangeEvent()
        {
            var hitCircle = new HitCircle();
            var editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap { HitObjects = { hitCircle } });

            HitObject changedObject = null;
            editorBeatmap.StartTimeChanged += h => changedObject = h;

            hitCircle.StartTime = 1000;
            Assert.That(changedObject, Is.EqualTo(hitCircle));
        }

        /// <summary>
        /// Tests that the changed event is correctly invoked after the start time of a hitobject is changed.
        /// This tests for hitobjects which were added to an existing editor beatmap.
        /// </summary>
        [Test]
        public void TestAddedHitObjectStartTimeChangeEvent()
        {
            var editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap());

            HitObject changedObject = null;
            editorBeatmap.StartTimeChanged += h => changedObject = h;

            var hitCircle = new HitCircle();

            editorBeatmap.Add(hitCircle);
            Assert.That(changedObject, Is.Null);

            hitCircle.StartTime = 1000;
            Assert.That(changedObject, Is.EqualTo(hitCircle));
        }

        /// <summary>
        /// Tests that the channged event is not invoked after a hitobject is removed from the beatmap/
        /// </summary>
        [Test]
        public void TestRemovedHitObjectStartTimeChangeEvent()
        {
            var hitCircle = new HitCircle();
            var editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap { HitObjects = { hitCircle } });

            HitObject changedObject = null;
            editorBeatmap.StartTimeChanged += h => changedObject = h;

            editorBeatmap.Remove(hitCircle);
            Assert.That(changedObject, Is.Null);

            hitCircle.StartTime = 1000;
            Assert.That(changedObject, Is.Null);
        }

        /// <summary>
        /// Tests that an added hitobject is correctly inserted to preserve the sorting order of the beatmap.
        /// </summary>
        [Test]
        public void TestAddHitObjectInMiddle()
        {
            var editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle(),
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 2000 },
                }
            });

            var hitCircle = new HitCircle { StartTime = 1000 };
            editorBeatmap.Add(hitCircle);
            Assert.That(editorBeatmap.HitObjects.Count(h => h == hitCircle), Is.EqualTo(1));
            Assert.That(editorBeatmap.HitObjects.IndexOf(hitCircle), Is.EqualTo(3));
        }

        /// <summary>
        /// Tests that the beatmap remains correctly sorted after the start time of a hitobject is changed.
        /// </summary>
        [Test]
        public void TestResortWhenStartTimeChanged()
        {
            var hitCircle = new HitCircle { StartTime = 1000 };
            var editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle(),
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 1000 },
                    hitCircle,
                    new HitCircle { StartTime = 2000 },
                }
            });

            hitCircle.StartTime = 0;
            Assert.That(editorBeatmap.HitObjects.Count(h => h == hitCircle), Is.EqualTo(1));
            Assert.That(editorBeatmap.HitObjects.IndexOf(hitCircle), Is.EqualTo(1));
        }
    }
}
