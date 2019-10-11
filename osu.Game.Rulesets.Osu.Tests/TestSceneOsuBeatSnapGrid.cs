// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneOsuBeatSnapGrid : ManualInputManagerTestScene
    {
        private const double beat_length = 100;
        private static readonly Vector2 grid_position = new Vector2(512, 384);

        [Cached(typeof(IEditorBeatmap))]
        private readonly EditorBeatmap<OsuHitObject> editorBeatmap;

        [Cached]
        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        private OsuBeatSnapGrid grid;
        private Drawable cursor;

        public TestSceneOsuBeatSnapGrid()
        {
            editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap());
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Clear();

            editorBeatmap.ControlPointInfo.TimingPoints.Clear();
            editorBeatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint { BeatLength = beat_length });

            beatDivisor.Value = 1;
        });

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            base.OnMouseMove(e);

            if (cursor != null)
                cursor.Position = grid?.GetSnapPosition(grid.ToLocalSpace(e.ScreenSpaceMousePosition)) ?? e.ScreenSpaceMousePosition;

            return true;
        }

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
            AddStep($"set beat divisor = {divisor}", () => beatDivisor.Value = divisor);
            createGrid();
        }

        private void createGrid()
        {
            AddStep("create grid", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    grid = new OsuBeatSnapGrid(new HitCircle { Position = grid_position }),
                    cursor = new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(50),
                        Colour = Color4.Red
                    }
                };
            });
        }

        private abstract class CircularBeatSnapGrid : BeatSnapGrid
        {
            protected override void CreateGrid(Vector2 startPosition)
            {
                float maxDistance = Math.Max(
                    Vector2.Distance(startPosition, Vector2.Zero),
                    Math.Max(
                        Vector2.Distance(startPosition, new Vector2(DrawWidth, 0)),
                        Math.Max(
                            Vector2.Distance(startPosition, new Vector2(0, DrawHeight)),
                            Vector2.Distance(startPosition, DrawSize))));

                int requiredCircles = (int)(maxDistance / DistanceSpacing);

                for (int i = 0; i < requiredCircles; i++)
                {
                    float radius = (i + 1) * DistanceSpacing * 2;

                    AddInternal(new CircularProgress
                    {
                        Origin = Anchor.Centre,
                        Position = startPosition,
                        Current = { Value = 1 },
                        Size = new Vector2(radius),
                        InnerRadius = 4 * 1f / radius,
                        Colour = GetColourForBeatIndex(i)
                    });
                }
            }

            public override Vector2 GetSnapPosition(Vector2 position)
            {
                Vector2 direction = position - StartPosition;
                float distance = direction.Length;

                float radius = DistanceSpacing;
                int radialCount = Math.Max(1, (int)Math.Round(distance / radius));

                if (radialCount <= 0)
                    return position;

                Vector2 normalisedDirection = direction * new Vector2(1f / distance);

                return StartPosition + normalisedDirection * radialCount * radius;
            }
        }

        private class OsuBeatSnapGrid : CircularBeatSnapGrid
        {
            /// <summary>
            /// Scoring distance with a speed-adjusted beat length of 1 second.
            /// </summary>
            private const float base_scoring_distance = 100;

            public OsuBeatSnapGrid(OsuHitObject hitObject)
                : base(hitObject, hitObject.StackedEndPosition)
            {
            }

            protected override float GetVelocity(double time, ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
            {
                TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(time);
                DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(time);

                double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

                return (float)(scoringDistance / timingPoint.BeatLength);
            }
        }
    }
}
