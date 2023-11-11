// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneBezierConverter : OsuTestScene
    {
        private readonly SmoothPath drawablePath;
        private readonly SmoothPath controlPointDrawablePath;
        private readonly SmoothPath convertedDrawablePath;
        private readonly SmoothPath convertedControlPointDrawablePath;

        private SliderPath path = null!;
        private SliderPath convertedPath = null!;

        public TestSceneBezierConverter()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Children =
                        new Drawable[]
                        {
                            drawablePath = new SmoothPath(),
                            controlPointDrawablePath = new SmoothPath
                            {
                                Colour = Colour4.Magenta,
                                PathRadius = 1f
                            }
                        },
                    Position = new Vector2(100)
                },
                new Container
                {
                    Children =
                        new Drawable[]
                        {
                            convertedDrawablePath = new SmoothPath(),
                            convertedControlPointDrawablePath = new SmoothPath
                            {
                                Colour = Colour4.Magenta,
                                PathRadius = 1f
                            }
                        },
                    Position = new Vector2(100, 300)
                }
            };

            resetPath();
        }

        [SetUp]
        public void Setup() => Schedule(resetPath);

        private void resetPath()
        {
            path = new SliderPath();
            convertedPath = new SliderPath();

            path.Version.ValueChanged += getConvertedControlPoints;
        }

        private void getConvertedControlPoints(ValueChangedEvent<int> obj)
        {
            convertedPath.ControlPoints.Clear();
            convertedPath.ControlPoints.AddRange(BezierConverter.ConvertToModernBezier(path.ControlPoints));
        }

        protected override void Update()
        {
            base.Update();

            List<Vector2> vertices = new List<Vector2>();

            path.GetPathToProgress(vertices, 0, 1);

            drawablePath.Vertices = vertices;
            controlPointDrawablePath.Vertices = path.ControlPoints.Select(o => o.Position).ToList();

            if (controlPointDrawablePath.Vertices.Count > 0)
            {
                controlPointDrawablePath.Position =
                    drawablePath.PositionInBoundingBox(drawablePath.Vertices[0]) - controlPointDrawablePath.PositionInBoundingBox(controlPointDrawablePath.Vertices[0]);
            }

            vertices.Clear();

            convertedPath.GetPathToProgress(vertices, 0, 1);

            convertedDrawablePath.Vertices = vertices;
            convertedControlPointDrawablePath.Vertices = convertedPath.ControlPoints.Select(o => o.Position).ToList();

            if (convertedControlPointDrawablePath.Vertices.Count > 0)
            {
                convertedControlPointDrawablePath.Position = convertedDrawablePath.PositionInBoundingBox(convertedDrawablePath.Vertices[0])
                                                             - convertedControlPointDrawablePath.PositionInBoundingBox(convertedControlPointDrawablePath.Vertices[0]);
            }
        }

        [Test]
        public void TestEmptyPath()
        {
        }

        [TestCase(SplineType.Linear, null)]
        [TestCase(SplineType.BSpline, null)]
        [TestCase(SplineType.BSpline, 3)]
        [TestCase(SplineType.Catmull, null)]
        [TestCase(SplineType.PerfectCurve, null)]
        public void TestSingleSegment(SplineType splineType, int? degree)
            => AddStep("create path", () => path.ControlPoints.AddRange(createSegment(new PathType { Type = splineType, Degree = degree }, Vector2.Zero, new Vector2(0, 100), new Vector2(100))));

        [TestCase(SplineType.Linear, null)]
        [TestCase(SplineType.BSpline, null)]
        [TestCase(SplineType.BSpline, 3)]
        [TestCase(SplineType.Catmull, null)]
        [TestCase(SplineType.PerfectCurve, null)]
        public void TestMultipleSegment(SplineType splineType, int? degree)
        {
            AddStep("create path", () =>
            {
                path.ControlPoints.AddRange(createSegment(PathType.LINEAR, Vector2.Zero));
                path.ControlPoints.AddRange(createSegment(new PathType { Type = splineType, Degree = degree }, new Vector2(0, 100), new Vector2(100), Vector2.Zero));
            });
        }

        [Test]
        public void TestComplex()
        {
            AddStep("create path", () =>
            {
                path.ControlPoints.AddRange(createSegment(PathType.LINEAR, Vector2.Zero, new Vector2(100, 0)));
                path.ControlPoints.AddRange(createSegment(PathType.BEZIER, new Vector2(100, 0), new Vector2(150, 30), new Vector2(100, 100)));
                path.ControlPoints.AddRange(createSegment(PathType.PERFECTCURVE, new Vector2(100, 100), new Vector2(25, 50), Vector2.Zero));
            });
        }

        [TestCase(0, 100)]
        [TestCase(1, 100)]
        [TestCase(5, 100)]
        [TestCase(10, 100)]
        [TestCase(30, 100)]
        [TestCase(50, 100)]
        [TestCase(100, 100)]
        [TestCase(100, 1)]
        public void TestPerfectCurveAngles(float height, float width)
        {
            AddStep("create path", () =>
            {
                path.ControlPoints.AddRange(createSegment(PathType.PERFECTCURVE, Vector2.Zero, new Vector2(width / 2, height), new Vector2(width, 0)));
            });
        }

        [TestCase(2)]
        [TestCase(4)]
        public void TestPerfectCurveFallbackScenarios(int points)
        {
            AddStep("create path", () =>
            {
                switch (points)
                {
                    case 2:
                        path.ControlPoints.AddRange(createSegment(PathType.PERFECTCURVE, Vector2.Zero, new Vector2(0, 100)));
                        break;

                    case 4:
                        path.ControlPoints.AddRange(createSegment(PathType.PERFECTCURVE, Vector2.Zero, new Vector2(0, 100), new Vector2(100), new Vector2(100, 0)));
                        break;
                }
            });
        }

        private List<PathControlPoint> createSegment(PathType type, params Vector2[] controlPoints)
        {
            var points = controlPoints.Select(p => new PathControlPoint { Position = p }).ToList();
            points[0].Type = type;
            return points;
        }
    }
}
