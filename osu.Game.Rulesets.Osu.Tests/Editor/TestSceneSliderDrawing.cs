// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneSliderDrawing : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new TestBeatmap(ruleset, false);
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint());
            return beatmap;
        }

        [Test]
        public void TestTouchInputPlaceHitCircleDirectly()
        {
            AddStep("tap circle", () => tap(this.ChildrenOfType<EditorRadioButton>().Single(b => b.Button.Label == "HitCircle")));

            AddStep("tap to place circle", () => tap(this.ChildrenOfType<Playfield>().Single()));
            AddAssert("circle placed correctly", () =>
            {
                var circle = (HitCircle)EditorBeatmap.HitObjects.Single(h => h.StartTime == EditorClock.CurrentTimeAccurate);
                Assert.Multiple(() =>
                {
                    Assert.That(circle.Position.X, Is.EqualTo(256f).Within(0.01f));
                    Assert.That(circle.Position.Y, Is.EqualTo(192f).Within(0.01f));
                });

                return true;
            });
        }

        [Test]
        public void TestTouchInputPlaceCircleAfterTouchingComposeArea()
        {
            AddStep("tap circle", () => tap(this.ChildrenOfType<EditorRadioButton>().Single(b => b.Button.Label == "HitCircle")));

            AddStep("tap playfield", () => tap(this.ChildrenOfType<Playfield>().Single()));
            AddAssert("circle placed", () => EditorBeatmap.HitObjects.Single(h => h.StartTime == EditorClock.CurrentTimeAccurate) is HitCircle);

            AddStep("move forward", () => InputManager.Key(Key.Right));

            AddStep("tap to place circle", () => tap(this.ChildrenOfType<Playfield>().Single()));
            AddAssert("circle placed correctly", () =>
            {
                var circle = (HitCircle)EditorBeatmap.HitObjects.Single(h => h.StartTime == EditorClock.CurrentTimeAccurate);
                Assert.Multiple(() =>
                {
                    Assert.That(circle.Position.X, Is.EqualTo(256f).Within(0.01f));
                    Assert.That(circle.Position.Y, Is.EqualTo(192f).Within(0.01f));
                });

                return true;
            });
        }

        [Test]
        public void TestTouchInputPlaceSliderDirectly()
        {
            AddStep("tap slider", () => tap(this.ChildrenOfType<EditorRadioButton>().Single(b => b.Button.Label == "Slider")));

            AddStep("hold to draw slider", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, this.ChildrenOfType<Playfield>().Single().ToScreenSpace(new Vector2(50, 20)))));
            AddStep("drag to draw", () => InputManager.MoveTouchTo(new Touch(TouchSource.Touch1, this.ChildrenOfType<Playfield>().Single().ToScreenSpace(new Vector2(200, 50)))));
            AddAssert("selection not initiated", () => this.ChildrenOfType<DragBox>().All(d => d.State == Visibility.Hidden));
            AddAssert("blueprint visible", () => this.ChildrenOfType<SliderPlacementBlueprint>().Single().Alpha > 0);
            AddStep("end", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, InputManager.CurrentState.Touch.GetTouchPosition(TouchSource.Touch1)!.Value)));
            AddAssert("slider placed correctly", () =>
            {
                var slider = (Slider)EditorBeatmap.HitObjects.Single(h => h.StartTime == EditorClock.CurrentTimeAccurate);
                Assert.Multiple(() =>
                {
                    Assert.That(slider.Position.X, Is.EqualTo(50f).Within(0.01f));
                    Assert.That(slider.Position.Y, Is.EqualTo(20f).Within(0.01f));
                    Assert.That(slider.Path.ControlPoints.Count, Is.EqualTo(2));
                    Assert.That(slider.Path.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));

                    // the final position may be slightly off from the mouse position when drawing, account for that.
                    Assert.That(slider.Path.ControlPoints[1].Position.X, Is.EqualTo(150).Within(5));
                    Assert.That(slider.Path.ControlPoints[1].Position.Y, Is.EqualTo(30).Within(5));
                });

                return true;
            });
        }

        [Test]
        public void TestTouchInputPlaceSliderAfterTouchingComposeArea()
        {
            AddStep("tap slider", () => tap(this.ChildrenOfType<EditorRadioButton>().Single(b => b.Button.Label == "Slider")));

            AddStep("tap playfield", () => tap(this.ChildrenOfType<Playfield>().Single()));
            AddStep("tap and hold another spot", () => hold(this.ChildrenOfType<Playfield>().Single(), new Vector2(50, 0)));
            AddUntilStep("wait for slider placement", () => EditorBeatmap.HitObjects.SingleOrDefault(h => h.StartTime == EditorClock.CurrentTimeAccurate) is Slider);
            AddStep("end", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, InputManager.CurrentState.Touch.GetTouchPosition(TouchSource.Touch1)!.Value)));

            AddStep("move forward", () => InputManager.Key(Key.Right));

            AddStep("hold to draw slider", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, this.ChildrenOfType<Playfield>().Single().ToScreenSpace(new Vector2(50, 20)))));
            AddStep("drag to draw", () => InputManager.MoveTouchTo(new Touch(TouchSource.Touch1, this.ChildrenOfType<Playfield>().Single().ToScreenSpace(new Vector2(200, 50)))));
            AddAssert("selection not initiated", () => this.ChildrenOfType<DragBox>().All(d => d.State == Visibility.Hidden));
            AddAssert("blueprint visible", () => this.ChildrenOfType<SliderPlacementBlueprint>().Single().IsPresent);
            AddStep("end", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, InputManager.CurrentState.Touch.GetTouchPosition(TouchSource.Touch1)!.Value)));
            AddAssert("slider placed correctly", () =>
            {
                var slider = (Slider)EditorBeatmap.HitObjects.Single(h => h.StartTime == EditorClock.CurrentTimeAccurate);
                Assert.Multiple(() =>
                {
                    Assert.That(slider.Position.X, Is.EqualTo(50f).Within(0.01f));
                    Assert.That(slider.Position.Y, Is.EqualTo(20f).Within(0.01f));
                    Assert.That(slider.Path.ControlPoints.Count, Is.EqualTo(2));
                    Assert.That(slider.Path.ControlPoints[0].Position, Is.EqualTo(Vector2.Zero));

                    // the final position may be slightly off from the mouse position when drawing, account for that.
                    Assert.That(slider.Path.ControlPoints[1].Position.X, Is.EqualTo(150).Within(5));
                    Assert.That(slider.Path.ControlPoints[1].Position.Y, Is.EqualTo(30).Within(5));
                });

                return true;
            });
        }

        private void tap(Drawable drawable, Vector2 offset = default) => tap(drawable.ToScreenSpace(drawable.LayoutRectangle.Centre + offset));

        private void tap(Vector2 position)
        {
            hold(position);
            InputManager.EndTouch(new Touch(TouchSource.Touch1, position));
        }

        private void hold(Drawable drawable, Vector2 offset = default) => hold(drawable.ToScreenSpace(drawable.LayoutRectangle.Centre + offset));

        private void hold(Vector2 position)
        {
            InputManager.BeginTouch(new Touch(TouchSource.Touch1, position));
        }
    }
}
