// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSliderSnapping : EditorTestScene
    {
        private const double beat_length = 1000;

        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint { BeatLength = beat_length });
            return new TestBeatmap(ruleset, false)
            {
                ControlPointInfo = controlPointInfo
            };
        }

        private Slider slider;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add unsnapped slider", () => EditorBeatmap.Add(slider = new Slider
            {
                StartTime = 0,
                Position = OsuPlayfield.BASE_SIZE / 5,
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(Vector2.Zero, PathType.PERFECTCURVE),
                        new PathControlPoint(new Vector2(136, 205)),
                        new PathControlPoint(new Vector2(-4, 226))
                    }
                }
            }));
            AddStep("set beat divisor to 1/1", () =>
            {
                var beatDivisor = (BindableBeatDivisor)Editor.Dependencies.Get(typeof(BindableBeatDivisor));
                beatDivisor.Value = 1;
            });
        }

        [Test]
        public void TestMovingUnsnappedSliderNodesSnaps()
        {
            PathControlPointPiece<Slider> sliderEnd = null;

            assertSliderSnapped(false);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("select slider end", () =>
            {
                sliderEnd = this.ChildrenOfType<PathControlPointPiece<Slider>>().Single(piece => piece.ControlPoint == slider.Path.ControlPoints.Last());
                InputManager.MoveMouseTo(sliderEnd.ScreenSpaceDrawQuad.Centre);
            });
            AddStep("move slider end", () =>
            {
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(sliderEnd.ScreenSpaceDrawQuad.Centre - new Vector2(0, 20));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            assertSliderSnapped(true);
        }

        [Test]
        public void TestAddingControlPointToUnsnappedSliderNodesSnaps()
        {
            assertSliderSnapped(false);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("move mouse to new point location", () =>
            {
                var firstPiece = this.ChildrenOfType<PathControlPointPiece<Slider>>().Single(piece => piece.ControlPoint == slider.Path.ControlPoints[0]);
                var pos = slider.Path.PositionAt(0.25d) + slider.Position;
                InputManager.MoveMouseTo(firstPiece.Parent!.ToScreenSpace(pos));
            });
            AddStep("move slider end", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            assertSliderSnapped(true);
        }

        [Test]
        public void TestRemovingControlPointFromUnsnappedSliderNodesSnaps()
        {
            assertSliderSnapped(false);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("move mouse to second control point", () =>
            {
                var secondPiece = this.ChildrenOfType<PathControlPointPiece<Slider>>().Single(piece => piece.ControlPoint == slider.Path.ControlPoints[1]);
                InputManager.MoveMouseTo(secondPiece);
            });
            AddStep("quick delete", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.PressButton(MouseButton.Right);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
            assertSliderSnapped(true);
        }

        [Test]
        public void TestResizingUnsnappedSliderSnaps()
        {
            SelectionBoxScaleHandle handle = null;

            assertSliderSnapped(false);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("move mouse to scale handle", () =>
            {
                handle = this.ChildrenOfType<SelectionBoxScaleHandle>().First();
                InputManager.MoveMouseTo(handle.ScreenSpaceDrawQuad.Centre);
            });
            AddStep("scale slider", () =>
            {
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(handle.ScreenSpaceDrawQuad.Centre + new Vector2(20, 20));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            assertSliderSnapped(true);
        }

        [Test]
        public void TestRotatingUnsnappedSliderDoesNotSnap()
        {
            SelectionBoxRotationHandle handle = null;

            assertSliderSnapped(false);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("move mouse to rotate handle", () =>
            {
                handle = this.ChildrenOfType<SelectionBoxRotationHandle>().First();
                InputManager.MoveMouseTo(handle.ScreenSpaceDrawQuad.Centre);
            });
            AddStep("scale slider", () =>
            {
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(handle.ScreenSpaceDrawQuad.Centre + new Vector2(0, 20));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            assertSliderSnapped(false);
        }

        [Test]
        public void TestRotatingSliderRetainsPerfectControlPointType()
        {
            OsuSelectionHandler selectionHandler;

            AddAssert("first control point perfect", () => slider.Path.ControlPoints[0].Type == PathType.PERFECTCURVE);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("rotate 90 degrees ccw", () =>
            {
                selectionHandler = this.ChildrenOfType<OsuSelectionHandler>().Single();
                selectionHandler.HandleRotation(-90);
            });

            AddAssert("first control point still perfect", () => slider.Path.ControlPoints[0].Type == PathType.PERFECTCURVE);
        }

        [Test]
        public void TestFlippingSliderDoesNotSnap()
        {
            OsuSelectionHandler selectionHandler = null;

            assertSliderSnapped(false);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("flip slider horizontally", () =>
            {
                selectionHandler = this.ChildrenOfType<OsuSelectionHandler>().Single();
                selectionHandler.OnPressed(new KeyBindingPressEvent<GlobalAction>(InputManager.CurrentState, GlobalAction.EditorFlipHorizontally));
            });

            assertSliderSnapped(false);

            AddStep("flip slider vertically", () =>
            {
                selectionHandler = this.ChildrenOfType<OsuSelectionHandler>().Single();
                selectionHandler.OnPressed(new KeyBindingPressEvent<GlobalAction>(InputManager.CurrentState, GlobalAction.EditorFlipVertically));
            });

            assertSliderSnapped(false);
        }

        [Test]
        public void TestFlippingSliderRetainsPerfectControlPointType()
        {
            OsuSelectionHandler selectionHandler;

            AddAssert("first control point perfect", () => slider.Path.ControlPoints[0].Type == PathType.PERFECTCURVE);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("flip slider horizontally", () =>
            {
                selectionHandler = this.ChildrenOfType<OsuSelectionHandler>().Single();
                selectionHandler.OnPressed(new KeyBindingPressEvent<GlobalAction>(InputManager.CurrentState, GlobalAction.EditorFlipVertically));
            });

            AddAssert("first control point still perfect", () => slider.Path.ControlPoints[0].Type == PathType.PERFECTCURVE);
        }

        [Test]
        public void TestReversingSliderDoesNotSnap()
        {
            assertSliderSnapped(false);

            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));
            AddStep("reverse slider", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            assertSliderSnapped(false);
        }

        private void assertSliderSnapped(bool snapped)
            => AddAssert($"slider is {(snapped ? "" : "not ")}snapped", () =>
            {
                double durationInBeatLengths = slider.Duration / beat_length;
                double fractionalPart = durationInBeatLengths - (int)durationInBeatLengths;
                return Precision.AlmostEquals(fractionalPart, 0) == snapped;
            });
    }
}
