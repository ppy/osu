// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneDistanceSnapGrid : EditorClockTestScene
    {
        private const double beat_length = 100;
        private const int beat_snap_distance = 10;

        private static readonly Vector2 grid_position = new Vector2(512, 384);

        private TestDistanceSnapGrid grid;

        [Cached(typeof(EditorBeatmap))]
        private readonly EditorBeatmap editorBeatmap;

        [Cached(typeof(IDistanceSnapProvider))]
        private readonly SnapProvider snapProvider = new SnapProvider();

        public TestSceneDistanceSnapGrid()
        {
            editorBeatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            });
            editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = beat_length });
            editorBeatmap.Difficulty.SliderMultiplier = 1;
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.SlateGray
                },
                grid = new TestDistanceSnapGrid()
            };
        });

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(12)]
        [TestCase(16)]
        public void TestBeatDivisor(int divisor)
        {
            AddStep($"set beat divisor = {divisor}", () => BeatDivisor.Value = divisor);
        }

        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void TestDistanceSpacing(double multiplier)
        {
            AddStep($"set distance spacing = {multiplier}", () => snapProvider.DistanceSpacingMultiplier.Value = multiplier);
            AddAssert("distance spacing matches multiplier", () => grid.DistanceBetweenTicks == beat_snap_distance * multiplier);
        }

        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void TestLimitedDistance(double multiplier)
        {
            const int end_time = 100;

            AddStep("create limited grid", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    grid = new TestDistanceSnapGrid(end_time)
                };
            });

            AddStep($"set distance spacing = {multiplier}", () => snapProvider.DistanceSpacingMultiplier.Value = multiplier);
            AddStep("check correct interval count", () => Assert.That((end_time / grid.DistanceBetweenTicks) * multiplier, Is.EqualTo(grid.MaxIntervals)));
        }

        private partial class TestDistanceSnapGrid : DistanceSnapGrid
        {
            public new float DistanceBetweenTicks => base.DistanceBetweenTicks;

            public new int MaxIntervals => base.MaxIntervals;

            public TestDistanceSnapGrid(double? endTime = null)
                : base(new HitObject(), grid_position, 0, endTime)
            {
            }

            protected override void CreateContent()
            {
                AddInternal(new Circle
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(5),
                    Position = StartPosition
                });

                int indexFromPlacement = 0;

                for (float s = StartPosition.X + DistanceBetweenTicks; s <= DrawWidth && indexFromPlacement < MaxIntervals; s += DistanceBetweenTicks, indexFromPlacement++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(5, 10),
                        Position = new Vector2(s, StartPosition.Y),
                        Colour = GetColourForIndexFromPlacement(indexFromPlacement)
                    });
                }

                indexFromPlacement = 0;

                for (float s = StartPosition.X - DistanceBetweenTicks; s >= 0 && indexFromPlacement < MaxIntervals; s -= DistanceBetweenTicks, indexFromPlacement++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(5, 10),
                        Position = new Vector2(s, StartPosition.Y),
                        Colour = GetColourForIndexFromPlacement(indexFromPlacement)
                    });
                }

                indexFromPlacement = 0;

                for (float s = StartPosition.Y + DistanceBetweenTicks; s <= DrawHeight && indexFromPlacement < MaxIntervals; s += DistanceBetweenTicks, indexFromPlacement++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(10, 5),
                        Position = new Vector2(StartPosition.X, s),
                        Colour = GetColourForIndexFromPlacement(indexFromPlacement)
                    });
                }

                indexFromPlacement = 0;

                for (float s = StartPosition.Y - DistanceBetweenTicks; s >= 0 && indexFromPlacement < MaxIntervals; s -= DistanceBetweenTicks, indexFromPlacement++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(10, 5),
                        Position = new Vector2(StartPosition.X, s),
                        Colour = GetColourForIndexFromPlacement(indexFromPlacement)
                    });
                }
            }

            public override (Vector2 position, double time) GetSnappedPosition(Vector2 screenSpacePosition)
                => (Vector2.Zero, 0);
        }

        private class SnapProvider : IDistanceSnapProvider
        {
            public Bindable<double> DistanceSpacingMultiplier { get; } = new BindableDouble(1);

            Bindable<double> IDistanceSnapProvider.DistanceSpacingMultiplier => DistanceSpacingMultiplier;

            public float GetBeatSnapDistanceAt(HitObject referenceObject, bool useReferenceSliderVelocity = true) => beat_snap_distance;

            public float DurationToDistance(HitObject referenceObject, double duration) => (float)duration;

            public double DistanceToDuration(HitObject referenceObject, float distance) => distance;

            public double FindSnappedDuration(HitObject referenceObject, float distance) => 0;

            public float FindSnappedDistance(HitObject referenceObject, float distance) => 0;
        }
    }
}
