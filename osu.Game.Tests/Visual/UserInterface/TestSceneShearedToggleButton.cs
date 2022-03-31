// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneShearedToggleButton : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestShearedToggleButton()
        {
            ShearedToggleButton button = null;

            AddStep("create button", () =>
            {
                Child = button = new ShearedToggleButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "Toggle me",
                    Width = 200
                };
            });

            AddToggleStep("toggle button", active => button.Active.Value = active);
            AddToggleStep("toggle disabled", disabled => button.Active.Disabled = disabled);
        }

        [Test]
        public void TestDisabledState()
        {
            ShearedToggleButton button = null;

            AddStep("create button", () =>
            {
                Child = button = new ShearedToggleButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "Toggle me",
                    Width = 200
                };
            });

            clickToggle();
            assertToggleState(true);

            clickToggle();
            assertToggleState(false);

            setToggleDisabledState(true);

            assertToggleState(false);
            clickToggle();
            assertToggleState(false);

            setToggleDisabledState(false);
            assertToggleState(false);
            clickToggle();
            assertToggleState(true);

            setToggleDisabledState(true);
            assertToggleState(true);
            clickToggle();
            assertToggleState(true);

            void clickToggle() => AddStep("click toggle", () =>
            {
                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            void assertToggleState(bool active) => AddAssert($"toggle is {(active ? "" : "not ")}active", () => button.Active.Value == active);

            void setToggleDisabledState(bool disabled) => AddStep($"{(disabled ? "disable" : "enable")} toggle", () => button.Active.Disabled = disabled);
        }
    }
}
