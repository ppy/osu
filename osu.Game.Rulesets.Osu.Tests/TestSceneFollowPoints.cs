// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
                hitObjectContainer = new Container<DrawableOsuHitObject> { RelativeSizeAxes = Axes.Both },
                followPointRenderer = new FollowPointRenderer { RelativeSizeAxes = Axes.Both }
            };
        });

        [Test]
        public void TestAddSingleHitCircle()
        {
            addObjects(() => new OsuHitObject[] { new HitCircle { Position = new Vector2(100, 100) } });
        }

        [Test]
        public void TestRemoveSingleHitCircle()
        {
            DrawableOsuHitObject obj = null;

            addObjects(() => new OsuHitObject[] { new HitCircle { Position = new Vector2(100, 100) } }, o => obj = o);
            removeObject(() => obj);
        }

        [Test]
        public void TestAddMultipleHitCircles()
        {
            addObjects(() => new OsuHitObject[]
            {
                new HitCircle { Position = new Vector2(100, 100) },
                new HitCircle { Position = new Vector2(200, 200) },
                new HitCircle { Position = new Vector2(300, 300) },
                new HitCircle { Position = new Vector2(400, 400) },
                new HitCircle { Position = new Vector2(500, 500) },
            });
        }

        [Test]
        public void TestRemoveEndHitCircle()
        {
            var objects = new List<DrawableOsuHitObject>();

            AddStep("reset", () => objects.Clear());

            addObjects(() => new OsuHitObject[]
            {
                new HitCircle { Position = new Vector2(100, 100) },
                new HitCircle { Position = new Vector2(200, 200) },
                new HitCircle { Position = new Vector2(300, 300) },
                new HitCircle { Position = new Vector2(400, 400) },
                new HitCircle { Position = new Vector2(500, 500) },
            }, o => objects.Add(o));

            removeObject(() => objects.Last());
        }

        [Test]
        public void TestRemoveStartHitCircle()
        {
            var objects = new List<DrawableOsuHitObject>();

            AddStep("reset", () => objects.Clear());

            addObjects(() => new OsuHitObject[]
            {
                new HitCircle { Position = new Vector2(100, 100) },
                new HitCircle { Position = new Vector2(200, 200) },
                new HitCircle { Position = new Vector2(300, 300) },
                new HitCircle { Position = new Vector2(400, 400) },
                new HitCircle { Position = new Vector2(500, 500) },
            }, o => objects.Add(o));

            removeObject(() => objects.First());
        }

        [Test]
        public void TestRemoveMiddleHitCircle()
        {
            var objects = new List<DrawableOsuHitObject>();

            AddStep("reset", () => objects.Clear());

            addObjects(() => new OsuHitObject[]
            {
                new HitCircle { Position = new Vector2(100, 100) },
                new HitCircle { Position = new Vector2(200, 200) },
                new HitCircle { Position = new Vector2(300, 300) },
                new HitCircle { Position = new Vector2(400, 400) },
                new HitCircle { Position = new Vector2(500, 500) },
            }, o => objects.Add(o));

            removeObject(() => objects[2]);
        }

        [Test]
        public void TestMoveHitCircle()
        {
            var objects = new List<DrawableOsuHitObject>();

            AddStep("reset", () => objects.Clear());

            addObjects(() => new OsuHitObject[]
            {
                new HitCircle { Position = new Vector2(100, 100) },
                new HitCircle { Position = new Vector2(200, 200) },
                new HitCircle { Position = new Vector2(300, 300) },
                new HitCircle { Position = new Vector2(400, 400) },
                new HitCircle { Position = new Vector2(500, 500) },
            }, o => objects.Add(o));

            AddStep("move hitobject", () => objects[2].HitObject.Position = new Vector2(300, 100));
        }

        private void addObjects(Func<OsuHitObject[]> ctorFunc, Action<DrawableOsuHitObject> storeFunc = null)
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

                    storeFunc?.Invoke(drawableObject);
                }
            });
        }

        private void removeObject(Func<DrawableOsuHitObject> getFunc)
        {
            AddStep("remove hitobject", () =>
            {
                var drawableObject = getFunc?.Invoke();

                hitObjectContainer.Remove(drawableObject);
                followPointRenderer.RemoveFollowPoints(drawableObject);
            });
        }
    }
}
