// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneBeatDivisorControl : OsuManualInputManagerTestScene
    {
        private BeatDivisorControl beatDivisorControl = null!;

        private SliderBar<int> tickSliderBar => beatDivisorControl.ChildrenOfType<SliderBar<int>>().Single();
        private Triangle tickMarkerHead => tickSliderBar.ChildrenOfType<Triangle>().Single();

        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Cached]
        private readonly BindableBeatDivisor bindableBeatDivisor = new BindableBeatDivisor(16);

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            bindableBeatDivisor.ValidDivisors.SetDefault();
            bindableBeatDivisor.SetDefault();

            Child = new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = beatDivisorControl = new BeatDivisorControl
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(90, 90),
                    Scale = new Vector2(3),
                }
            };
        });

        [Test]
        public void TestBindableBeatDivisor()
        {
            AddRepeatStep("move previous", () => bindableBeatDivisor.SelectPrevious(), 2);
            AddAssert("divisor is 4", () => bindableBeatDivisor.Value == 4);
            AddRepeatStep("move next", () => bindableBeatDivisor.SelectNext(), 1);
            AddAssert("divisor is 12", () => bindableBeatDivisor.Value == 8);
        }

        [Test]
        public void TestMouseInput()
        {
            AddStep("hold marker", () =>
            {
                InputManager.MoveMouseTo(tickMarkerHead.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("move to 1", () => InputManager.MoveMouseTo(getPositionForDivisor(1)));
            AddStep("move to 16 and release", () =>
            {
                InputManager.MoveMouseTo(getPositionForDivisor(16));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("divisor is 16", () => bindableBeatDivisor.Value == 16);
            AddStep("hold marker", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move to ~6 and release", () =>
            {
                InputManager.MoveMouseTo(getPositionForDivisor(6));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("divisor clamped to 8", () => bindableBeatDivisor.Value == 8);
            AddStep("move to ~10 and click", () =>
            {
                InputManager.MoveMouseTo(getPositionForDivisor(10));
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("divisor clamped to 8", () => bindableBeatDivisor.Value == 8);
        }

        private Vector2 getPositionForDivisor(int divisor)
        {
            float localX = (1 - 1 / (float)divisor) * tickSliderBar.UsableWidth + tickSliderBar.RangePadding;
            return tickSliderBar.ToScreenSpace(new Vector2(
                localX,
                tickSliderBar.DrawHeight / 2
            ));
        }

        [Test]
        public void TestBeatChevronNavigation()
        {
            switchBeatSnap(1);
            assertBeatSnap(16);

            switchBeatSnap(-4);
            assertBeatSnap(1);

            switchBeatSnap(3);
            assertBeatSnap(8);

            switchBeatSnap(3);
            assertBeatSnap(16);

            switchBeatSnap(-2);
            assertBeatSnap(4);

            switchBeatSnap(-3);
            assertBeatSnap(1);
        }

        [Test]
        public void TestKeyboardNavigation()
        {
            pressKey(1);
            assertBeatSnap(1);
            assertPreset(BeatDivisorType.Common);

            pressKey(2);
            assertBeatSnap(2);
            assertPreset(BeatDivisorType.Common);

            pressKey(3);
            assertBeatSnap(3);
            assertPreset(BeatDivisorType.Triplets);

            pressKey(4);
            assertBeatSnap(4);
            assertPreset(BeatDivisorType.Common);

            pressKey(5);
            assertBeatSnap(5);
            assertPreset(BeatDivisorType.Custom, 5);

            pressKey(6);
            assertBeatSnap(6);
            assertPreset(BeatDivisorType.Triplets);

            pressKey(7);
            assertBeatSnap(7);
            assertPreset(BeatDivisorType.Custom, 7);

            pressKey(8);
            assertBeatSnap(8);
            assertPreset(BeatDivisorType.Common);

            void pressKey(int key) => AddStep($"press shift+{key}", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.Number0 + key);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
        }

        [Test]
        public void TestBeatPresetNavigation()
        {
            assertPreset(BeatDivisorType.Common);

            switchPresets(1);
            assertPreset(BeatDivisorType.Triplets);
            assertBeatSnap(6);

            switchPresets(1);
            assertPreset(BeatDivisorType.Common);
            assertBeatSnap(4);

            switchPresets(-1);
            assertPreset(BeatDivisorType.Triplets);

            switchPresets(-1);
            assertPreset(BeatDivisorType.Common);

            setDivisorViaInput(3);
            assertPreset(BeatDivisorType.Triplets);

            setDivisorViaInput(8);
            assertPreset(BeatDivisorType.Common);

            setDivisorViaInput(15);
            assertPreset(BeatDivisorType.Custom, 15);
            assertBeatSnap(15);

            switchBeatSnap(-1);
            assertBeatSnap(5);

            switchBeatSnap(-1);
            assertBeatSnap(3);

            setDivisorViaInput(5);
            assertPreset(BeatDivisorType.Custom, 15);
            assertBeatSnap(5);

            switchPresets(1);
            assertPreset(BeatDivisorType.Common);

            switchPresets(-1);
            assertPreset(BeatDivisorType.Custom, 15);
            assertBeatSnap(15);
        }

        private void switchBeatSnap(int direction) => AddRepeatStep($"move snap {(direction > 0 ? "forward" : "backward")}", () =>
        {
            int chevronIndex = direction > 0 ? 1 : 0;
            var chevronButton = beatDivisorControl.ChildrenOfType<BeatDivisorControl.ChevronButton>().ElementAt(chevronIndex);
            InputManager.MoveMouseTo(chevronButton);
            InputManager.Click(MouseButton.Left);
        }, Math.Abs(direction));

        private void assertBeatSnap(int expected) => AddAssert($"beat snap is {expected}",
            () => bindableBeatDivisor.Value, () => Is.EqualTo(expected));

        private void switchPresets(int direction) => AddRepeatStep($"move presets {(direction > 0 ? "forward" : "backward")}", () =>
        {
            int chevronIndex = direction > 0 ? 3 : 2;
            var chevronButton = beatDivisorControl.ChildrenOfType<BeatDivisorControl.ChevronButton>().ElementAt(chevronIndex);
            InputManager.MoveMouseTo(chevronButton);
            InputManager.Click(MouseButton.Left);
        }, Math.Abs(direction));

        private void assertPreset(BeatDivisorType type, int? maxDivisor = null)
        {
            AddAssert($"preset is {type}", () => bindableBeatDivisor.ValidDivisors.Value.Type, () => Is.EqualTo(type));

            if (type == BeatDivisorType.Custom)
            {
                Debug.Assert(maxDivisor != null);
                AddAssert($"max divisor is {maxDivisor}", () => bindableBeatDivisor.ValidDivisors.Value.Presets.Max() == maxDivisor.Value);
            }
        }

        private void setDivisorViaInput(int divisor)
        {
            AddStep("open divisor input popover", () =>
            {
                var button = beatDivisorControl.ChildrenOfType<BeatDivisorControl.DivisorDisplay>().Single();
                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            BeatDivisorControl.CustomDivisorPopover? popover = null;
            AddUntilStep("wait for popover", () => (popover = this.ChildrenOfType<BeatDivisorControl.CustomDivisorPopover>().SingleOrDefault()) != null && popover.IsLoaded);
            AddStep($"set divisor to {divisor}", () =>
            {
                var textBox = popover.ChildrenOfType<TextBox>().Single();
                InputManager.MoveMouseTo(textBox);
                InputManager.Click(MouseButton.Left);
                textBox.Text = divisor.ToString();
                InputManager.Key(Key.Enter);
            });
            AddUntilStep("wait for dismiss", () => !this.ChildrenOfType<BeatDivisorControl.CustomDivisorPopover>().Any());
        }
    }
}
