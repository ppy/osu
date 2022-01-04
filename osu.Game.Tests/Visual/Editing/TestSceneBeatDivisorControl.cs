// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
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

        private SliderBar<int> tickSliderBar;
        private EquilateralTriangle tickMarkerHead;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = beatDivisorControl = new BeatDivisorControl(bindableBeatDivisor = new BindableBeatDivisor(16))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(90, 90)
            };

            tickSliderBar = beatDivisorControl.ChildrenOfType<SliderBar<int>>().Single();
            tickMarkerHead = tickSliderBar.ChildrenOfType<EquilateralTriangle>().Single();
        });

        [Test]
        public void TestBindableBeatDivisor()
        {
            AddRepeatStep("move previous", () => bindableBeatDivisor.Previous(), 4);
            AddAssert("divisor is 4", () => bindableBeatDivisor.Value == 4);
            AddRepeatStep("move next", () => bindableBeatDivisor.Next(), 3);
            AddAssert("divisor is 12", () => bindableBeatDivisor.Value == 12);
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
    }
}
