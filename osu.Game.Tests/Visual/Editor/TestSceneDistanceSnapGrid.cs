// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editor
{
    public class TestSceneDistanceSnapGrid : EditorClockTestScene
    {
        private const double beat_length = 100;
        private static readonly Vector2 grid_position = new Vector2(512, 384);

        [Cached(typeof(IEditorBeatmap))]
        private readonly EditorBeatmap<OsuHitObject> editorBeatmap;

        [Cached(typeof(IDistanceSnapProvider))]
        private readonly SnapProvider snapProvider = new SnapProvider();

        public TestSceneDistanceSnapGrid()
        {
            editorBeatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap());
            editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = beat_length });

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.SlateGray
                },
                new TestDistanceSnapGrid(new HitObject(), grid_position)
            };
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
            AddStep($"set beat divisor = {divisor}", () => BeatDivisor.Value = divisor);
        }

        private class TestDistanceSnapGrid : DistanceSnapGrid
        {
            public new float DistanceSpacing => base.DistanceSpacing;

            public TestDistanceSnapGrid(HitObject hitObject, Vector2 centrePosition)
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

            public override (Vector2 position, double time) GetSnappedPosition(Vector2 screenSpacePosition)
                => (Vector2.Zero, 0);
        }

        private class SnapProvider : IDistanceSnapProvider
        {
            public (Vector2 position, double time) GetSnappedPosition(Vector2 position, double time) => (position, time);

            public float GetBeatSnapDistanceAt(double referenceTime) => 10;

            public float DurationToDistance(double referenceTime, double duration) => 0;

            public double DistanceToDuration(double referenceTime, float distance) => 0;

            public double GetSnappedDurationFromDistance(double referenceTime, float distance) => 0;

            public float GetSnappedDistanceFromDistance(double referenceTime, float distance) => 0;
        }
    }
}
