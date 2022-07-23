// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    public class TestSceneBeatDivisorControl : OsuManualInputManagerTestScene
    {
        private BeatDivisorControl beatDivisorControl;
        private BindableBeatDivisor bindableBeatDivisor;

        private SliderBar<int> tickSliderBar => beatDivisorControl.ChildrenOfType<SliderBar<int>>().Single();
        private Triangle tickMarkerHead => tickSliderBar.ChildrenOfType<Triangle>().Single();

        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = beatDivisorControl = new BeatDivisorControl(bindableBeatDivisor = new BindableBeatDivisor(16))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(90, 90)
                }
            };
        });

        [Test]
        public void TestBindableBeatDivisor()
        {
            AddRepeatStep("move previous", () => bindableBeatDivisor.Previous(), 2);
            AddAssert("divisor is 4", () => bindableBeatDivisor.Value == 4);
            AddRepeatStep("move next", () => bindableBeatDivisor.Next(), 1);
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
            AddStep("move to 8 and release", () =>
            {
                InputManager.MoveMouseTo(tickSliderBar.ScreenSpaceDrawQuad.Centre);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("divisor is 8", () => bindableBeatDivisor.Value == 8);
            AddStep("hold marker", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move to 16", () => InputManager.MoveMouseTo(getPositionForDivisor(16)));
            AddStep("move to ~10 and release", () =>
            {
                InputManager.MoveMouseTo(getPositionForDivisor(10));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("divisor clamped to 8", () => bindableBeatDivisor.Value == 8);
        }

        private Vector2 getPositionForDivisor(int divisor)
        {
            float relativePosition = (float)Math.Clamp(divisor, 0, 16) / 16;
            var sliderDrawQuad = tickSliderBar.ScreenSpaceDrawQuad;
            return new Vector2(
                sliderDrawQuad.TopLeft.X + sliderDrawQuad.Width * relativePosition,
                sliderDrawQuad.Centre.Y
            );
        }

        [Test]
        public void TestBeatChevronNavigation()
        {
            switchBeatSnap(1);
            assertBeatSnap(1);

            switchBeatSnap(3);
            assertBeatSnap(8);

            switchBeatSnap(-1);
            assertBeatSnap(4);

            switchBeatSnap(-3);
            assertBeatSnap(16);
        }

        [Test]
        public void TestBeatPresetNavigation()
        {
            assertPreset(BeatDivisorType.Common);

            switchPresets(1);
            assertPreset(BeatDivisorType.Triplets);

            switchPresets(1);
            assertPreset(BeatDivisorType.Common);

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

            switchBeatSnap(-1);
            assertBeatSnap(5);

            switchBeatSnap(-1);
            assertBeatSnap(3);

            setDivisorViaInput(5);
            assertPreset(BeatDivisorType.Custom, 15);

            switchPresets(1);
            assertPreset(BeatDivisorType.Common);

            switchPresets(-1);
            assertPreset(BeatDivisorType.Triplets);
        }

        private void switchBeatSnap(int direction) => AddRepeatStep($"move snap {(direction > 0 ? "forward" : "backward")}", () =>
        {
            int chevronIndex = direction > 0 ? 1 : 0;
            var chevronButton = beatDivisorControl.ChildrenOfType<BeatDivisorControl.ChevronButton>().ElementAt(chevronIndex);
            InputManager.MoveMouseTo(chevronButton);
            InputManager.Click(MouseButton.Left);
        }, Math.Abs(direction));

        private void assertBeatSnap(int expected) => AddAssert($"beat snap is {expected}",
            () => bindableBeatDivisor.Value == expected);

        private void switchPresets(int direction) => AddRepeatStep($"move presets {(direction > 0 ? "forward" : "backward")}", () =>
        {
            int chevronIndex = direction > 0 ? 3 : 2;
            var chevronButton = beatDivisorControl.ChildrenOfType<BeatDivisorControl.ChevronButton>().ElementAt(chevronIndex);
            InputManager.MoveMouseTo(chevronButton);
            InputManager.Click(MouseButton.Left);
        }, Math.Abs(direction));

        private void assertPreset(BeatDivisorType type, int? maxDivisor = null)
        {
            AddAssert($"preset is {type}", () => bindableBeatDivisor.ValidDivisors.Value.Type == type);

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

            BeatDivisorControl.CustomDivisorPopover popover = null;
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
