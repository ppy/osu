// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Types;
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

        private class FollowPointRenderer : CompositeDrawable
        {
            /// <summary>
            /// Adds the <see cref="FollowPoint"/>s around a <see cref="DrawableOsuHitObject"/>.
            /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
            /// </summary>
            /// <param name="hitObject">The <see cref="DrawableOsuHitObject"/> to add <see cref="FollowPoint"/>s for.</param>
            public void AddFollowPoints(DrawableOsuHitObject hitObject)
            {
                var startGroup = new FollowPointGroup(hitObject);
                AddInternal(startGroup);

                // Groups are sorted by their start time when added, so the index can be used to post-process other surrounding groups
                int startIndex = IndexOfInternal(startGroup);

                if (startIndex < InternalChildren.Count - 1)
                {
                    //     h1 -> -> -> h2
                    //  hitObject   nextGroup

                    var nextGroup = (FollowPointGroup)InternalChildren[startIndex + 1];
                    startGroup.End = nextGroup.Start;
                }

                if (startIndex > 0)
                {
                    //     h1 -> -> -> h2
                    //  prevGroup   hitObject

                    var previousGroup = (FollowPointGroup)InternalChildren[startIndex - 1];
                    previousGroup.End = startGroup.Start;
                }
            }

            /// <summary>
            /// Removes the <see cref="FollowPoint"/>s around a <see cref="DrawableOsuHitObject"/>.
            /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
            /// </summary>
            /// <param name="hitObject">The <see cref="DrawableOsuHitObject"/> to remove <see cref="FollowPoint"/>s for.</param>
            public void RemoveFollowPoints(DrawableOsuHitObject hitObject)
            {
                var groups = findGroups(hitObject);

                // Regardless of the position of the hitobject in the beatmap, there will always be a group leading from the hitobject
                RemoveInternal(groups.start);

                if (groups.end != null)
                {
                    // When there were two groups referencing the same hitobject,  merge them by updating the end group to point to the new end (the start group was already removed)
                    groups.end.End = groups.start.End;
                }
            }

            /// <summary>
            /// Finds the <see cref="FollowPointGroup"/>s with <paramref name="hitObject"/> as the start and end <see cref="DrawableOsuHitObject"/>s.
            /// </summary>
            /// <param name="hitObject">The <see cref="DrawableOsuHitObject"/> to find the relevant <see cref="FollowPointGroup"/> of.</param>
            /// <returns>A tuple containing the end group (the <see cref="FollowPointGroup"/> where <paramref name="hitObject"/> is the end of),
            /// and the start group (the <see cref="FollowPointGroup"/> where <paramref name="hitObject"/> is the start of).</returns>
            private (FollowPointGroup start, FollowPointGroup end) findGroups(DrawableOsuHitObject hitObject)
            {
                //           endGroup         startGroup
                //     h1 -> -> -> -> -> h2 -> -> -> -> -> h3
                //                    hitObject

                FollowPointGroup startGroup = null; // The group which the hitobject is the start in
                FollowPointGroup endGroup = null; // The group which the hitobject is the end in

                int startIndex = 0;

                for (; startIndex < InternalChildren.Count; startIndex++)
                {
                    var group = (FollowPointGroup)InternalChildren[startIndex];

                    if (group.Start == hitObject)
                    {
                        startGroup = group;
                        break;
                    }
                }

                if (startIndex > 0)
                    endGroup = (FollowPointGroup)InternalChildren[startIndex - 1];

                return (startGroup, endGroup);
            }

            protected override int Compare(Drawable x, Drawable y)
            {
                var groupX = (FollowPointGroup)x;
                var groupY = (FollowPointGroup)y;

                return groupX.Start.HitObject.StartTime.CompareTo(groupY.Start.HitObject.StartTime);
            }
        }

        private class FollowPointGroup : CompositeDrawable
        {
            // Todo: These shouldn't be constants
            private const int spacing = 32;
            private const double preempt = 800;

            /// <summary>
            /// The <see cref="DrawableOsuHitObject"/> which <see cref="FollowPoint"/>s will exit from.
            /// </summary>
            [NotNull]
            public readonly DrawableOsuHitObject Start;

            public FollowPointGroup(DrawableOsuHitObject start)
            {
                Start = start;
                RelativeSizeAxes = Axes.Both;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                bindHitObject(Start);
            }

            private DrawableOsuHitObject end;

            /// <summary>
            /// The <see cref="DrawableOsuHitObject"/> which <see cref="FollowPoint"/>s will enter.
            /// </summary>
            [CanBeNull]
            public DrawableOsuHitObject End
            {
                get => end;
                set
                {
                    end = value;

                    if (end != null)
                        bindHitObject(end);

                    refreshFollowPoints();
                }
            }

            private void bindHitObject(DrawableOsuHitObject drawableObject)
            {
                drawableObject.HitObject.StartTimeBindable.BindValueChanged(_ => refreshFollowPoints());
                drawableObject.HitObject.PositionBindable.BindValueChanged(_ => refreshFollowPoints());
                drawableObject.HitObject.DefaultsApplied += refreshFollowPoints;
            }

            private void refreshFollowPoints()
            {
                ClearInternal();

                if (End == null)
                    return;

                OsuHitObject osuStart = Start.HitObject;
                OsuHitObject osuEnd = End.HitObject;

                if (osuEnd.NewCombo)
                    return;

                if (osuStart is Spinner || osuEnd is Spinner)
                    return;

                Vector2 startPosition = osuStart.EndPosition;
                Vector2 endPosition = osuEnd.Position;
                double startTime = (osuStart as IHasEndTime)?.EndTime ?? osuStart.StartTime;
                double endTime = osuEnd.StartTime;

                Vector2 distanceVector = endPosition - startPosition;
                int distance = (int)distanceVector.Length;
                float rotation = (float)(Math.Atan2(distanceVector.Y, distanceVector.X) * (180 / Math.PI));
                double duration = endTime - startTime;

                for (int d = (int)(spacing * 1.5); d < distance - spacing; d += spacing)
                {
                    float fraction = (float)d / distance;
                    Vector2 pointStartPosition = startPosition + (fraction - 0.1f) * distanceVector;
                    Vector2 pointEndPosition = startPosition + fraction * distanceVector;
                    double fadeOutTime = startTime + fraction * duration;
                    double fadeInTime = fadeOutTime - preempt;

                    FollowPoint fp;

                    AddInternal(fp = new FollowPoint
                    {
                        Position = pointStartPosition,
                        Rotation = rotation,
                        Alpha = 0,
                        Scale = new Vector2(1.5f * osuEnd.Scale),
                    });

                    using (fp.BeginAbsoluteSequence(fadeInTime))
                    {
                        fp.FadeIn(osuEnd.TimeFadeIn);
                        fp.ScaleTo(osuEnd.Scale, osuEnd.TimeFadeIn, Easing.Out);
                        fp.MoveTo(pointEndPosition, osuEnd.TimeFadeIn, Easing.Out);
                        fp.Delay(fadeOutTime - fadeInTime).FadeOut(osuEnd.TimeFadeIn);
                    }

                    fp.Expire(true);
                }
            }
        }
    }
}
