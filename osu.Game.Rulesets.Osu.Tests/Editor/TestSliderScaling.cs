// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSliderScaling : TestSceneOsuEditor
    {
        private OsuPlayfield playfield;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("get playfield", () => playfield = Editor.ChildrenOfType<OsuPlayfield>().First());
            AddStep("seek to first timing point", () => EditorClock.Seek(Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.First().Time));
        }

        [Test]
        public void TestScalingLinearSlider()
        {
            Slider slider = null;

            AddStep("Add slider", () =>
            {
                slider = new Slider { StartTime = EditorClock.CurrentTime, Position = new Vector2(300) };

                PathControlPoint[] points =
                {
                    new PathControlPoint(new Vector2(0), PathType.LINEAR),
                    new PathControlPoint(new Vector2(100, 0)),
                };

                slider.Path = new SliderPath(points);
                EditorBeatmap.Add(slider);
            });

            AddAssert("ensure object placed", () => EditorBeatmap.HitObjects.Count == 1);

            moveMouse(new Vector2(300));
            AddStep("select slider", () => InputManager.Click(MouseButton.Left));

            double distanceBefore = 0;

            AddStep("store distance", () => distanceBefore = slider.Path.Distance);

            AddStep("move mouse to handle", () => InputManager.MoveMouseTo(Editor.ChildrenOfType<SelectionBoxDragHandle>().Skip(1).First()));
            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));
            moveMouse(new Vector2(300, 300));
            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("slider length shrunk", () => slider.Path.Distance < distanceBefore);
        }

        private void moveMouse(Vector2 pos) =>
            AddStep($"move mouse to {pos}", () => InputManager.MoveMouseTo(playfield.ToScreenSpace(pos)));
    }

    [TestFixture]
    public class TestSliderNearLinearScaling
    {
        private readonly Random rng = new Random(1337);

        [Test]
        public void TestScalingSliderFlat()
        {
            SliderPath sliderPathPerfect = new SliderPath(
            [
                new PathControlPoint(new Vector2(0), PathType.PERFECT_CURVE),
                new PathControlPoint(new Vector2(50, 25)),
                new PathControlPoint(new Vector2(25, 100)),
            ]);

            SliderPath sliderPathBezier = new SliderPath(
            [
                new PathControlPoint(new Vector2(0), PathType.BEZIER),
                new PathControlPoint(new Vector2(50, 25)),
                new PathControlPoint(new Vector2(25, 100)),
            ]);

            scaleSlider(sliderPathPerfect, new Vector2(0.000001f, 1));
            scaleSlider(sliderPathBezier, new Vector2(0.000001f, 1));

            for (int i = 0; i < 100; i++)
            {
                Assert.True(Precision.AlmostEquals(sliderPathPerfect.PositionAt(i / 100.0f), sliderPathBezier.PositionAt(i / 100.0f)));
            }
        }

        [Test]
        public void TestPerfectCurveMatchesTheoretical()
        {
            for (int i = 0; i < 20000; i++)
            {
                //Only test points that are in the screen's bounds
                float p1X = 640.0f * (float)rng.NextDouble();
                float p2X = 640.0f * (float)rng.NextDouble();

                float p1Y = 480.0f * (float)rng.NextDouble();
                float p2Y = 480.0f * (float)rng.NextDouble();
                SliderPath sliderPathPerfect = new SliderPath(
                [
                    new PathControlPoint(new Vector2(0, 0), PathType.PERFECT_CURVE),
                    new PathControlPoint(new Vector2(p1X, p1Y)),
                    new PathControlPoint(new Vector2(p2X, p2Y)),
                ]);

                assertMatchesPerfectCircle(sliderPathPerfect);

                scaleSlider(sliderPathPerfect, new Vector2(0.00001f, 1));

                assertMatchesPerfectCircle(sliderPathPerfect);
            }
        }

        private void assertMatchesPerfectCircle(SliderPath path)
        {
            if (path.ControlPoints.Count != 3)
                return;

            //Replication of PathApproximator.CircularArcToPiecewiseLinear
            CircularArcProperties circularArcProperties = new CircularArcProperties(path.ControlPoints.Select(x => x.Position).ToArray());

            if (!circularArcProperties.IsValid)
                return;

            //Addresses cases where circularArcProperties.ThetaRange>0.5
            //Occurs in code in PathControlPointVisualiser.ensureValidPathType
            RectangleF boundingBox = PathApproximator.CircularArcBoundingBox(path.ControlPoints.Select(x => x.Position).ToArray());
            if (boundingBox.Width >= 640 || boundingBox.Height >= 480)
                return;

            int subpoints = (2f * circularArcProperties.Radius <= 0.1f) ? 2 : Math.Max(2, (int)Math.Ceiling(circularArcProperties.ThetaRange / (2.0 * Math.Acos(1f - (0.1f / circularArcProperties.Radius)))));

            //ignore cases where subpoints is int.MaxValue, result will be garbage
            //as well, having this many subpoints will cause an out of memory error, so can't happen during normal useage
            if (subpoints == int.MaxValue)
                return;

            for (int i = 0; i < Math.Min(subpoints, 100); i++)
            {
                float progress = (float)rng.NextDouble();

                //To avoid errors from interpolating points, ensure we check only positions that would be subpoints.
                progress = (float)Math.Ceiling(progress * (subpoints - 1)) / (subpoints - 1);

                //Special case - if few subpoints, ensure checking every single one rather than randomly
                if (subpoints < 100)
                    progress = i / (float)(subpoints - 1);

                //edge points cause issue with interpolation, so ignore the last two points and first
                if (progress == 0.0f || progress >= (subpoints - 2) / (float)(subpoints - 1))
                    continue;

                double theta = circularArcProperties.ThetaStart + (circularArcProperties.Direction * progress * circularArcProperties.ThetaRange);
                Vector2 vector = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * circularArcProperties.Radius;

                Assert.True(Precision.AlmostEquals(circularArcProperties.Centre + vector, path.PositionAt(progress), 0.01f),
                    "A perfect circle with points " + string.Join(", ", path.ControlPoints.Select(x => x.Position)) + " and radius" + circularArcProperties.Radius + "from SliderPath does not almost equal a theoretical perfect circle with " + subpoints + " subpoints"
                    + ": " + (circularArcProperties.Centre + vector) + " - " + path.PositionAt(progress)
                    + " = " + (circularArcProperties.Centre + vector - path.PositionAt(progress))
                );
            }
        }

        private void scaleSlider(SliderPath path, Vector2 scale)
        {
            for (int i = 0; i < path.ControlPoints.Count; i++)
            {
                path.ControlPoints[i].Position *= scale;
            }
        }
    }
}
