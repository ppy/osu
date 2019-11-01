// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneFollowPoints : OsuTestScene
    {
        private Container<DrawableHitObject> hitObjectContainer;
        private FollowPointRenderer followPointRenderer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                hitObjectContainer = new Container<DrawableHitObject> { RelativeSizeAxes = Axes.Both },
                followPointRenderer = new FollowPointRenderer { RelativeSizeAxes = Axes.Both }
            };
        });

        [Test]
        public void TestAddSingleHitObject()
        {
            addObject(new HitCircle { Position = new Vector2(100, 100) });
        }

        [Test]
        public void TestRemoveSingleHitObject()
        {
        }

        [Test]
        public void TestAddMultipleHitObjects()
        {
        }

        [Test]
        public void TestRemoveEndObject()
        {
        }

        [Test]
        public void TestRemoveStartObject()
        {
        }

        [Test]
        public void TestRemoveMiddleObject()
        {
        }

        private void addObject(HitCircle hitCircle, Action<DrawableHitObject> func = null)
        {
            AddStep("add hitobject", () =>
            {
                hitCircle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                var drawableCircle = new DrawableHitCircle(hitCircle);

                hitObjectContainer.Add(drawableCircle);
                followPointRenderer.AddFollowPoints(drawableCircle);

                func?.Invoke(drawableCircle);
            });
        }

        private void removeObject(Func<DrawableHitObject> func)
        {
            AddStep("remove hitobject", () =>
            {
                var drawableObject = func?.Invoke();

                hitObjectContainer.Remove(drawableObject);
                followPointRenderer.RemoveFollowPoints(drawableObject);
            });
        }

        private class FollowPointRenderer : CompositeDrawable
        {
            /// <summary>
            /// Adds the <see cref="FollowPoint"/>s around a <see cref="DrawableHitObject"/>.
            /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
            /// </summary>
            /// <param name="hitObject">The <see cref="DrawableHitObject"/> to add <see cref="FollowPoint"/>s for.</param>
            public void AddFollowPoints(DrawableHitObject hitObject)
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
            /// Removes the <see cref="FollowPoint"/>s around a <see cref="DrawableHitObject"/>.
            /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
            /// </summary>
            /// <param name="hitObject">The <see cref="DrawableHitObject"/> to remove <see cref="FollowPoint"/>s for.</param>
            public void RemoveFollowPoints(DrawableHitObject hitObject)
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
            /// Finds the <see cref="FollowPointGroup"/>s with <paramref name="hitObject"/> as the start and end <see cref="DrawableHitObject"/>s.
            /// </summary>
            /// <param name="hitObject">The <see cref="DrawableHitObject"/> to find the relevant <see cref="FollowPointGroup"/> of.</param>
            /// <returns>A tuple containing the end group (the <see cref="FollowPointGroup"/> where <paramref name="hitObject"/> is the end of),
            /// and the start group (the <see cref="FollowPointGroup"/> where <paramref name="hitObject"/> is the start of).</returns>
            private (FollowPointGroup start, FollowPointGroup end) findGroups(DrawableHitObject hitObject)
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
            private const int spacing = 32;
            private const double preempt = 800;

            public FollowPointGroup(DrawableHitObject start)
            {
                Start = start;
            }

            /// <summary>
            /// The <see cref="DrawableHitObject"/> which <see cref="FollowPoint"/>s will exit from.
            /// </summary>
            [NotNull]
            public DrawableHitObject Start { get; }

            private DrawableHitObject end;

            /// <summary>
            /// The <see cref="DrawableHitObject"/> which <see cref="FollowPoint"/>s will enter.
            /// </summary>
            [CanBeNull]
            public DrawableHitObject End
            {
                get => end;
                set
                {
                    end = value;
                    refreshFollowPoints();
                }
            }

            private void refreshFollowPoints()
            {
                ClearInternal();

                if (End == null)
                    return;

                OsuHitObject osuStart = (OsuHitObject)Start.HitObject;
                OsuHitObject osuEnd = (OsuHitObject)End.HitObject;

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

        private class FollowPoint : CompositeDrawable
        {
            private const float width = 8;

            public override bool RemoveWhenNotAlive => false;

            public FollowPoint()
            {
                Origin = Anchor.Centre;

                InternalChild = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.FollowPoint), _ => new Container
                {
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = width / 2,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Color4.White.Opacity(0.2f),
                        Radius = 4,
                    },
                    Child = new Box
                    {
                        Size = new Vector2(width),
                        Blending = BlendingParameters.Additive,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Alpha = 0.5f,
                    }
                }, confineMode: ConfineMode.NoScaling);
            }
        }
    }
}
