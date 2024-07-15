// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneOsuEditor : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestTapSliderButtonThenDragAfterTouchingComposeArea()
        {
            AddStep("tap slider", () => tap(this.ChildrenOfType<EditorRadioButton>().Single(b => b.Button.Label == "Slider")));

            // this input is just for interacting with compose area
            AddStep("begin hold", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, this.ChildrenOfType<Playfield>().Single().ScreenSpaceDrawQuad.Centre)));
            AddStep("drag to draw", () => InputManager.MoveTouchTo(new Touch(TouchSource.Touch1, this.ChildrenOfType<Playfield>().Single().ToScreenSpace(new Vector2(200, 50)))));
            AddStep("end", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, InputManager.CurrentState.Touch.GetTouchPosition(TouchSource.Touch1)!.Value)));

            AddStep("move current time", () => InputManager.Key(Key.Right));

            AddStep("begin hold", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, this.ChildrenOfType<Playfield>().Single().ToScreenSpace(new Vector2(50, 20)))));
            AddStep("drag to draw", () => InputManager.MoveTouchTo(new Touch(TouchSource.Touch1, this.ChildrenOfType<Playfield>().Single().ToScreenSpace(new Vector2(200, 50)))));
            AddAssert("selection not initiated", () => this.ChildrenOfType<DragBox>().All(d => d.State == Visibility.Hidden));
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

        private void tap(Drawable drawable)
        {
            InputManager.BeginTouch(new Touch(TouchSource.Touch1, drawable.ScreenSpaceDrawQuad.Centre));
            InputManager.EndTouch(new Touch(TouchSource.Touch1, drawable.ScreenSpaceDrawQuad.Centre));
        }
    }
}
