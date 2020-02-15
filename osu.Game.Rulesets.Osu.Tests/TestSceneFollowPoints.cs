// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Connections;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneFollowPoints : OsuTestScene
    {
        private Container<DrawableOsuHitObject> hitObjectContainer;
        private FollowPointRenderer followPointRenderer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                hitObjectContainer = new TestHitObjectContainer { RelativeSizeAxes = Axes.Both },
                followPointRenderer = new FollowPointRenderer { RelativeSizeAxes = Axes.Both }
            };
        });

        [Test]
        public void TestAddObject()
        {
            addObjectsStep(() => new OsuHitObject[] { new HitCircle { Position = new Vector2(100, 100) } });

            assertGroups();
        }

        [Test]
        public void TestRemoveObject()
        {
            addObjectsStep(() => new OsuHitObject[] { new HitCircle { Position = new Vector2(100, 100) } });

            removeObjectStep(() => getObject(0));

            assertGroups();
        }

        [Test]
        public void TestAddMultipleObjects()
        {
            addMultipleObjectsStep();

            assertGroups();
        }

        [Test]
        public void TestRemoveEndObject()
        {
            addMultipleObjectsStep();

            removeObjectStep(() => getObject(4));

            assertGroups();
        }

        [Test]
        public void TestRemoveStartObject()
        {
            addMultipleObjectsStep();

            removeObjectStep(() => getObject(0));

            assertGroups();
        }

        [Test]
        public void TestRemoveMiddleObject()
        {
            addMultipleObjectsStep();

            removeObjectStep(() => getObject(2));

            assertGroups();
        }

        [Test]
        public void TestMoveObject()
        {
            addMultipleObjectsStep();

            AddStep("move hitobject", () => getObject(2).HitObject.Position = new Vector2(300, 100));

            assertGroups();
        }

        [TestCase(0, 0)] // Start -> Start
        [TestCase(0, 2)] // Start -> Middle
        [TestCase(0, 5)] // Start -> End
        [TestCase(2, 0)] // Middle -> Start
        [TestCase(1, 3)] // Middle -> Middle (forwards)
        [TestCase(3, 1)] // Middle -> Middle (backwards)
        [TestCase(4, 0)] // End -> Start
        [TestCase(4, 2)] // End -> Middle
        [TestCase(4, 4)] // End -> End
        public void TestReorderObjects(int startIndex, int endIndex)
        {
            addMultipleObjectsStep();

            reorderObjectStep(startIndex, endIndex);

            assertGroups();
        }

        private void addMultipleObjectsStep() => addObjectsStep(() => new OsuHitObject[]
        {
            new HitCircle { Position = new Vector2(100, 100) },
            new HitCircle { Position = new Vector2(200, 200) },
            new HitCircle { Position = new Vector2(300, 300) },
            new HitCircle { Position = new Vector2(400, 400) },
            new HitCircle { Position = new Vector2(500, 500) },
        });

        private void addObjectsStep(Func<OsuHitObject[]> ctorFunc)
        {
            AddStep("add hitobjects", () =>
            {
                var objects = ctorFunc();

                for (int i = 0; i < objects.Length; i++)
                {
                    objects[i].StartTime = Time.Current + 1000 + 500 * (i + 1);
                    objects[i].ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                    DrawableOsuHitObject drawableObject = null;

                    switch (objects[i])
                    {
                        case HitCircle circle:
                            drawableObject = new DrawableHitCircle(circle);
                            break;

                        case Slider slider:
                            drawableObject = new DrawableSlider(slider);
                            break;

                        case Spinner spinner:
                            drawableObject = new DrawableSpinner(spinner);
                            break;
                    }

                    hitObjectContainer.Add(drawableObject);
                    followPointRenderer.AddFollowPoints(drawableObject);
                }
            });
        }

        private void removeObjectStep(Func<DrawableOsuHitObject> getFunc)
        {
            AddStep("remove hitobject", () =>
            {
                var drawableObject = getFunc?.Invoke();

                hitObjectContainer.Remove(drawableObject);
                followPointRenderer.RemoveFollowPoints(drawableObject);
            });
        }

        private void reorderObjectStep(int startIndex, int endIndex)
        {
            AddStep($"move object {startIndex} to {endIndex}", () =>
            {
                DrawableOsuHitObject toReorder = getObject(startIndex);

                double targetTime;
                if (endIndex < hitObjectContainer.Count)
                    targetTime = getObject(endIndex).HitObject.StartTime - 1;
                else
                    targetTime = getObject(hitObjectContainer.Count - 1).HitObject.StartTime + 1;

                hitObjectContainer.Remove(toReorder);
                toReorder.HitObject.StartTime = targetTime;
                hitObjectContainer.Add(toReorder);
            });
        }

        private void assertGroups()
        {
            AddAssert("has correct group count", () => followPointRenderer.Connections.Count == hitObjectContainer.Count);
            AddAssert("group endpoints are correct", () =>
            {
                for (int i = 0; i < hitObjectContainer.Count; i++)
                {
                    DrawableOsuHitObject expectedStart = getObject(i);
                    DrawableOsuHitObject expectedEnd = i < hitObjectContainer.Count - 1 ? getObject(i + 1) : null;

                    if (getGroup(i).Start != expectedStart)
                        throw new AssertionException($"Object {i} expected to be the start of group {i}.");

                    if (getGroup(i).End != expectedEnd)
                        throw new AssertionException($"Object {(expectedEnd == null ? "null" : i.ToString())} expected to be the end of group {i}.");
                }

                return true;
            });
        }

        private DrawableOsuHitObject getObject(int index) => hitObjectContainer[index];

        private FollowPointConnection getGroup(int index) => followPointRenderer.Connections[index];

        private class TestHitObjectContainer : Container<DrawableOsuHitObject>
        {
            protected override int Compare(Drawable x, Drawable y)
            {
                var osuX = (DrawableOsuHitObject)x;
                var osuY = (DrawableOsuHitObject)y;

                int compare = osuX.HitObject.StartTime.CompareTo(osuY.HitObject.StartTime);

                if (compare == 0)
                    return base.Compare(x, y);

                return compare;
            }
        }
    }
}
