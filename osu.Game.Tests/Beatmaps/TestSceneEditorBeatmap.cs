// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Beatmaps
{
    [HeadlessTest]
    public class TestSceneEditorBeatmap : EditorClockTestScene
    {
        /// <summary>
        /// Tests that the addition event is correctly invoked after a hitobject is added.
        /// </summary>
        [Test]
        public void TestHitObjectAddEvent()
        {
            var editorBeatmap = new EditorBeatmap(new OsuBeatmap());

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
            var editorBeatmap = new EditorBeatmap(new OsuBeatmap { HitObjects = { hitCircle } });

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

            HitObject changedObject = null;

            AddStep("add beatmap", () =>
            {
                EditorBeatmap editorBeatmap;

                Child = editorBeatmap = new EditorBeatmap(new OsuBeatmap { HitObjects = { hitCircle } });
                editorBeatmap.HitObjectUpdated += h => changedObject = h;
            });

            AddStep("change start time", () => hitCircle.StartTime = 1000);
            AddAssert("received change event", () => changedObject == hitCircle);
        }

        /// <summary>
        /// Tests that the changed event is correctly invoked after the start time of a hitobject is changed.
        /// This tests for hitobjects which were added to an existing editor beatmap.
        /// </summary>
        [Test]
        public void TestAddedHitObjectStartTimeChangeEvent()
        {
            EditorBeatmap editorBeatmap = null;
            HitObject changedObject = null;

            AddStep("add beatmap", () =>
            {
                Child = editorBeatmap = new EditorBeatmap(new OsuBeatmap());
                editorBeatmap.HitObjectUpdated += h => changedObject = h;
            });

            var hitCircle = new HitCircle();

            AddStep("add object", () => editorBeatmap.Add(hitCircle));
            AddAssert("event not received", () => changedObject == null);

            AddStep("change start time", () => hitCircle.StartTime = 1000);
            AddAssert("event received", () => changedObject == hitCircle);
        }

        /// <summary>
        /// Tests that the channged event is not invoked after a hitobject is removed from the beatmap/
        /// </summary>
        [Test]
        public void TestRemovedHitObjectStartTimeChangeEvent()
        {
            var hitCircle = new HitCircle();
            var editorBeatmap = new EditorBeatmap(new OsuBeatmap { HitObjects = { hitCircle } });

            HitObject changedObject = null;
            editorBeatmap.HitObjectUpdated += h => changedObject = h;

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
            var editorBeatmap = new EditorBeatmap(new OsuBeatmap
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
            var editorBeatmap = new EditorBeatmap(new OsuBeatmap
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

        /// <summary>
        /// Tests that multiple hitobjects are updated simultaneously.
        /// </summary>
        [Test]
        public void TestMultipleHitObjectUpdate()
        {
            var updatedObjects = new List<HitObject>();
            var allHitObjects = new List<HitObject>();
            EditorBeatmap editorBeatmap = null;

            AddStep("add beatmap", () =>
            {
                updatedObjects.Clear();

                Child = editorBeatmap = new EditorBeatmap(new OsuBeatmap());

                for (int i = 0; i < 10; i++)
                {
                    var h = new HitCircle();
                    editorBeatmap.Add(h);
                    allHitObjects.Add(h);
                }
            });

            AddStep("change all start times", () =>
            {
                editorBeatmap.HitObjectUpdated += h => updatedObjects.Add(h);

                for (int i = 0; i < 10; i++)
                    allHitObjects[i].StartTime += 10;
            });

            // Distinct ensures that all hitobjects have been updated once, debounce is tested below.
            AddAssert("all hitobjects updated", () => updatedObjects.Distinct().Count() == 10);
        }

        /// <summary>
        /// Tests that hitobject updates are debounced when they happen too soon.
        /// </summary>
        [Test]
        public void TestDebouncedUpdate()
        {
            var updatedObjects = new List<HitObject>();
            EditorBeatmap editorBeatmap = null;

            AddStep("add beatmap", () =>
            {
                updatedObjects.Clear();

                Child = editorBeatmap = new EditorBeatmap(new OsuBeatmap());
                editorBeatmap.Add(new HitCircle());
            });

            AddStep("change start time twice", () =>
            {
                editorBeatmap.HitObjectUpdated += h => updatedObjects.Add(h);

                editorBeatmap.HitObjects[0].StartTime = 10;
                editorBeatmap.HitObjects[0].StartTime = 20;
            });

            AddAssert("only updated once", () => updatedObjects.Count == 1);
        }
    }
}
