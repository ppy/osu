// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editor
{
    public class TestSceneBeatSnapGrid : EditorClockTestScene
    {
        private const double beat_length = 100;
        private static readonly Vector2 grid_position = new Vector2(512, 384);

        [Cached(typeof(IEditorBeatmap))]
        private readonly EditorBeatmap<OsuHitObject> editorBeatmap;

        private TestBeatSnapGrid grid;

        public TestSceneBeatSnapGrid()
        {
            editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap());
            editorBeatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint { BeatLength = beat_length });

            createGrid();
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Clear();

            editorBeatmap.ControlPointInfo.TimingPoints.Clear();
            editorBeatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint { BeatLength = beat_length });

            BeatDivisor.Value = 1;
        });

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(12)]
        [TestCase(16)]
        public void TestInitialBeatDivisor(int divisor)
        {
            AddStep($"set beat divisor = {divisor}", () => BeatDivisor.Value = divisor);
            createGrid();

            float expectedDistance = (float)beat_length / divisor;
            AddAssert($"spacing is {expectedDistance}", () => Precision.AlmostEquals(grid.DistanceSpacing, expectedDistance));
        }

        [Test]
        public void TestChangeBeatDivisor()
        {
            createGrid();
            AddStep("set beat divisor = 2", () => BeatDivisor.Value = 2);

            const float expected_distance = (float)beat_length / 2;
            AddAssert($"spacing is {expected_distance}", () => Precision.AlmostEquals(grid.DistanceSpacing, expected_distance));
        }

        [TestCase(100)]
        [TestCase(200)]
        public void TestBeatLength(double beatLength)
        {
            AddStep($"set beat length = {beatLength}", () =>
            {
                editorBeatmap.ControlPointInfo.TimingPoints.Clear();
                editorBeatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint { BeatLength = beatLength });
            });

            createGrid();
            AddAssert($"spacing is {beatLength}", () => Precision.AlmostEquals(grid.DistanceSpacing, beatLength));
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TestGridVelocity(float velocity)
        {
            createGrid(g => g.Velocity = velocity);

            float expectedDistance = (float)beat_length * velocity;
            AddAssert($"spacing is {expectedDistance}", () => Precision.AlmostEquals(grid.DistanceSpacing, expectedDistance));
        }

        [Test]
        public void TestGetSnappedTime()
        {
            createGrid();

            Vector2 snapPosition = Vector2.Zero;
            AddStep("get first tick position", () => snapPosition = grid_position + new Vector2((float)beat_length, 0));
            AddAssert("snap time is 1 beat away", () => Precision.AlmostEquals(beat_length, grid.GetSnapTime(snapPosition), 0.01));

            createGrid(g => g.Velocity = 2, "with velocity = 2");
            AddAssert("snap time is now 0.5 beats away", () => Precision.AlmostEquals(beat_length / 2, grid.GetSnapTime(snapPosition), 0.01));
        }

        private void createGrid(Action<TestBeatSnapGrid> func = null, string description = null)
        {
            AddStep($"create grid {description ?? string.Empty}", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    grid = new TestBeatSnapGrid(new HitObject(), grid_position)
                };

                func?.Invoke(grid);
            });
        }

        private class TestBeatSnapGrid : BeatSnapGrid
        {
            public new float Velocity = 1;

            public new float DistanceSpacing => base.DistanceSpacing;

            public TestBeatSnapGrid(HitObject hitObject, Vector2 centrePosition)
                : base(hitObject, centrePosition)
            {
            }

            protected override void CreateContent(Vector2 centrePosition)
            {
                AddInternal(new Circle
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(5),
                    Position = centrePosition
                });

                int beatIndex = 0;

                for (float s = centrePosition.X + DistanceSpacing; s <= DrawWidth; s += DistanceSpacing, beatIndex++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(5, 10),
                        Position = new Vector2(s, centrePosition.Y),
                        Colour = GetColourForBeatIndex(beatIndex)
                    });
                }

                beatIndex = 0;

                for (float s = centrePosition.X - DistanceSpacing; s >= 0; s -= DistanceSpacing, beatIndex++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(5, 10),
                        Position = new Vector2(s, centrePosition.Y),
                        Colour = GetColourForBeatIndex(beatIndex)
                    });
                }

                beatIndex = 0;

                for (float s = centrePosition.Y + DistanceSpacing; s <= DrawHeight; s += DistanceSpacing, beatIndex++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(10, 5),
                        Position = new Vector2(centrePosition.X, s),
                        Colour = GetColourForBeatIndex(beatIndex)
                    });
                }

                beatIndex = 0;

                for (float s = centrePosition.Y - DistanceSpacing; s >= 0; s -= DistanceSpacing, beatIndex++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(10, 5),
                        Position = new Vector2(centrePosition.X, s),
                        Colour = GetColourForBeatIndex(beatIndex)
                    });
                }
            }

            protected override float GetVelocity(double time, ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
                => Velocity;

            public override Vector2 GetSnapPosition(Vector2 screenSpacePosition)
                => Vector2.Zero;
        }
    }
}
