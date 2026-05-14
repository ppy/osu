// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSliderVelocityAdjust : OsuGameTestScene
    {
        private Screens.Edit.Editor? editor => Game.ScreenStack.CurrentScreen as Screens.Edit.Editor;

        private EditorBeatmap editorBeatmap => editor.ChildrenOfType<EditorBeatmap>().FirstOrDefault()!;

        private EditorClock editorClock => editor.ChildrenOfType<EditorClock>().FirstOrDefault()!;

        private Slider? slider => editorBeatmap.HitObjects.OfType<Slider>().FirstOrDefault();

        private TimelineHitObjectBlueprint blueprint => editor.ChildrenOfType<TimelineHitObjectBlueprint>().FirstOrDefault()!;

        private DifficultyPointPiece difficultyPointPiece => blueprint.ChildrenOfType<DifficultyPointPiece>().First();

        private IndeterminateSliderWithTextBoxInput<double> velocityTextBox => Game.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().First().ChildrenOfType<IndeterminateSliderWithTextBoxInput<double>>().First();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [TestCase(true)]
        [TestCase(false)]
        public void TestVelocityChangeSavesCorrectly(bool adjustVelocity)
        {
            double? velocity = null;

            AddStep("enter editor", () => Game.ScreenStack.Push(new EditorLoader()));
            AddUntilStep("wait for editor load", () => editor?.ReadyForUse, () => Is.True);

            AddStep("seek to first control point", () => editorClock.Seek(editorBeatmap.ControlPointInfo.TimingPoints.First().Time));
            AddStep("enter slider placement mode", () => InputManager.Key(Key.Number3));

            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(editor.ChildrenOfType<Playfield>().First().ScreenSpaceDrawQuad.Centre));
            AddStep("start placement", () => InputManager.Click(MouseButton.Left));

            AddStep("move mouse to bottom right", () => InputManager.MoveMouseTo(editor.ChildrenOfType<Playfield>().First().ScreenSpaceDrawQuad.BottomRight - new Vector2(10)));
            AddStep("end placement", () => InputManager.Click(MouseButton.Right));

            AddStep("exit placement mode", () => InputManager.Key(Key.Number1));

            AddAssert("slider placed", () => slider, () => Is.Not.Null);

            AddStep("select slider", () => editorBeatmap.SelectedHitObjects.Add(slider));

            AddAssert("ensure one slider placed", () => slider, () => Is.Not.Null);

            AddStep("store velocity", () => velocity = slider!.Velocity);

            if (adjustVelocity)
            {
                AddStep("open velocity adjust panel", () => difficultyPointPiece.TriggerClick());
                AddStep("change velocity", () => velocityTextBox.Current.Value = 2);

                AddAssert("velocity adjusted", () => slider!.Velocity,
                    () => Is.EqualTo(velocity!.Value * 2).Within(Precision.DOUBLE_EPSILON));

                AddStep("store velocity", () => velocity = slider!.Velocity);
            }

            AddStep("save", () => InputManager.Keys(PlatformAction.Save));
            AddStep("exit", () => InputManager.Key(Key.Escape));

            AddStep("enter editor (again)", () => Game.ScreenStack.Push(new EditorLoader()));
            AddUntilStep("wait for editor load", () => editor?.ReadyForUse, () => Is.True);

            AddStep("seek to slider", () => editorClock.Seek(slider!.StartTime));
            AddAssert("slider has correct velocity", () => slider!.Velocity, () => Is.EqualTo(velocity));
        }

        [Test]
        public void TestVelocityUndo()
        {
            double? velocityBefore = null;
            double? durationBefore = null;

            AddStep("enter editor", () => Game.ScreenStack.Push(new EditorLoader()));
            AddUntilStep("wait for editor load", () => editor?.ReadyForUse == true);

            AddStep("seek to first control point", () => editorClock.Seek(editorBeatmap.ControlPointInfo.TimingPoints.First().Time));
            AddStep("enter slider placement mode", () => InputManager.Key(Key.Number3));

            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(editor.ChildrenOfType<Playfield>().First().ScreenSpaceDrawQuad.Centre));
            AddStep("start placement", () => InputManager.Click(MouseButton.Left));

            AddStep("move mouse to bottom right", () => InputManager.MoveMouseTo(editor.ChildrenOfType<Playfield>().First().ScreenSpaceDrawQuad.BottomRight - new Vector2(10)));
            AddStep("end placement", () => InputManager.Click(MouseButton.Right));

            AddStep("exit placement mode", () => InputManager.Key(Key.Number1));

            AddAssert("slider placed", () => slider, () => Is.Not.Null);
            AddStep("select slider", () => editorBeatmap.SelectedHitObjects.Add(slider));

            AddStep("store velocity", () =>
            {
                velocityBefore = slider!.Velocity;
                durationBefore = slider.Duration;
            });

            AddStep("open velocity adjust panel", () => difficultyPointPiece.TriggerClick());
            AddStep("change velocity", () => velocityTextBox.Current.Value = 2);

            AddAssert("velocity adjusted", () => slider!.Velocity, () => Is.EqualTo(velocityBefore!.Value * 2).Within(Precision.DOUBLE_EPSILON));

            AddStep("undo", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Z);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddAssert("slider has correct velocity", () => slider!.Velocity, () => Is.EqualTo(velocityBefore));
            AddAssert("slider has correct duration", () => slider!.Duration, () => Is.EqualTo(durationBefore));
        }

        [Test]
        public void TestVelocityToolbox()
        {
            ExpandableSlider<double> velocitySlider = null!;
            ExpandableButton useLastSliderButton = null!;

            AddStep("enter editor", () => Game.ScreenStack.Push(new EditorLoader()));
            AddUntilStep("wait for editor load", () => editor?.ReadyForUse == true);
            AddStep("retrieve controls", () =>
            {
                var toolbox = this.ChildrenOfType<OsuSliderVelocityToolboxGroup>().Single();
                velocitySlider = toolbox.ChildrenOfType<ExpandableSlider<double>>().Single();
                useLastSliderButton = toolbox.ChildrenOfType<ExpandableButton>().Single();
            });

            AddAssert("velocity slider at 1x", () => velocitySlider.Current.Value, () => Is.EqualTo(1));
            AddAssert("use last slider button disabled", () => useLastSliderButton.Enabled.Value, () => Is.False);

            AddStep("seek to 5000", () => editorClock.Seek(5000));
            AddStep("set 2x velocity", () => velocitySlider.Current.Value = 2);
            placeSlider();
            AddAssert("placed slider has 2x velocity", () => editorBeatmap.HitObjects.OfType<Slider>().Last().SliderVelocityMultiplier, () => Is.EqualTo(2));
            AddAssert("use last slider button disabled", () => useLastSliderButton.Enabled.Value, () => Is.False);

            AddStep("seek to 6000", () => editorClock.Seek(6000));
            placeSlider();
            AddAssert("placed slider has 2x velocity", () => editorBeatmap.HitObjects.OfType<Slider>().Last().SliderVelocityMultiplier, () => Is.EqualTo(2));
            AddAssert("use last slider button disabled", () => useLastSliderButton.Enabled.Value, () => Is.False);

            AddStep("seek to 9000", () => editorClock.Seek(9000));
            AddStep("set 3x velocity", () => velocitySlider.Current.Value = 3);
            placeSlider();
            AddAssert("placed slider has 3x velocity", () => editorBeatmap.HitObjects.OfType<Slider>().Last().SliderVelocityMultiplier, () => Is.EqualTo(3));
            AddAssert("use last slider button disabled", () => useLastSliderButton.Enabled.Value, () => Is.False);

            AddStep("seek to 10000", () => editorClock.Seek(10000));
            AddStep("set 1x velocity", () => velocitySlider.Current.Value = 1);
            AddStep("use last slider velocity instead", () => useLastSliderButton.TriggerClick());
            placeSlider();
            AddAssert("placed slider has 3x velocity", () => editorBeatmap.HitObjects.OfType<Slider>().Last().SliderVelocityMultiplier, () => Is.EqualTo(3));
            AddAssert("use last slider button disabled", () => useLastSliderButton.Enabled.Value, () => Is.False);

            AddStep("seek back to 7000", () => editorClock.Seek(7000));
            placeSlider();
            AddAssert("placed slider has 2x velocity", () => editorBeatmap.HitObjects.OfType<Slider>().ElementAt(2).SliderVelocityMultiplier, () => Is.EqualTo(2));
            AddAssert("use last slider button disabled", () => useLastSliderButton.Enabled.Value, () => Is.False);

            void placeSlider()
            {
                AddStep("enter slider placement mode", () => InputManager.Key(Key.Number3));
                AddStep("move mouse to top left", () => InputManager.MoveMouseTo(editor.ChildrenOfType<Playfield>().First().ScreenSpaceDrawQuad.TopLeft + new Vector2(50)));
                AddStep("start placement", () => InputManager.Click(MouseButton.Left));
                AddStep("move mouse to bottom right", () => InputManager.MoveMouseTo(editor.ChildrenOfType<Playfield>().First().ScreenSpaceDrawQuad.BottomRight - new Vector2(50)));
                AddStep("end placement", () => InputManager.Click(MouseButton.Right));
            }
        }
    }
}
