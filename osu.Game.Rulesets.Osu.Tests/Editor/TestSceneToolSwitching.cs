// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneToolSwitching : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestSliderAnchorMoveOperationEndsOnSwitchingTool()
        {
            var initialPosition = Vector2.Zero;

            AddStep("store original anchor position", () => initialPosition = EditorBeatmap.HitObjects.OfType<Slider>().First().Path.ControlPoints.ElementAt(1).Position);
            AddStep("select first slider", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.OfType<Slider>().First()));
            AddStep("move to second anchor", () => InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(1)));
            AddStep("start dragging", () => InputManager.PressButton(MouseButton.Left));
            AddStep("drag away", () => InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(0, -200)));
            AddStep("switch tool", () => InputManager.PressButton(MouseButton.Button1));
            AddStep("undo", () => Editor.Undo());
            AddAssert("anchor back at original position",
                () => EditorBeatmap.HitObjects.OfType<Slider>().First().Path.ControlPoints.ElementAt(1).Position,
                () => Is.EqualTo(initialPosition));
        }

        [Test]
        public void TestSliderAnchorCreationOperationEndsOnSwitchingTool()
        {
            AddStep("select first slider", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.OfType<Slider>().First()));
            AddStep("move to second anchor", () => InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(1), new Vector2(-50, 0)));
            AddStep("quick-create anchor", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddStep("drag away", () => InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(0, -200)));
            AddStep("switch tool", () => InputManager.PressKey(Key.Number3));
            AddStep("drag away further", () => InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(0, -200)));
            AddStep("select first slider", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.OfType<Slider>().First()));
            AddStep("undo", () => Editor.Undo());
            AddAssert("slider has three anchors again", () => EditorBeatmap.HitObjects.OfType<Slider>().First().Path.ControlPoints, () => Has.Count.EqualTo(3));
        }
    }
}
