// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
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
        [Test]
        public void TestScalingSliderFlat()
        {
            Slider sliderPerfect = new Slider
            {
                Position = new Vector2(300),
                Path = new SliderPath(
                    [
                        new PathControlPoint(new Vector2(0), PathType.PERFECT_CURVE),
                        new PathControlPoint(new Vector2(50, 25)),
                        new PathControlPoint(new Vector2(25, 100)),
                    ])
            };

            Slider sliderBezier = new Slider
            {
                Position = new Vector2(300),
                Path = new SliderPath(
                    [
                        new PathControlPoint(new Vector2(0), PathType.BEZIER),
                        new PathControlPoint(new Vector2(50, 25)),
                        new PathControlPoint(new Vector2(25, 100)),
                    ])
            };

            scaleSlider(sliderPerfect, new Vector2(0.000001f, 1));
            scaleSlider(sliderBezier, new Vector2(0.000001f, 1));

            for (int i = 0; i < 100; i++)
            {
                Assert.True(Precision.AlmostEquals(sliderPerfect.Path.PositionAt(i / 100.0f), sliderBezier.Path.PositionAt(i / 100.0f)));
            }
        }

        private void scaleSlider(Slider slider, Vector2 scale)
        {
            for (int i = 0; i < slider.Path.ControlPoints.Count; i++)
            {
                slider.Path.ControlPoints[i].Position *= scale;
            }
        }
    }
}
