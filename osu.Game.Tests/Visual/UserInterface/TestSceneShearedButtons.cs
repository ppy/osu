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
    public class TestSceneShearedButtons : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [TestCase(false)]
        [TestCase(true)]
        public void TestShearedButton(bool bigButton)
        {
            ShearedButton button = null;
            bool actionFired = false;

            AddStep("create button", () =>
            {
                actionFired = false;

                if (bigButton)
                {
                    Child = button = new ShearedButton(400)
                    {
                        LighterColour = Colour4.FromHex("#FFFFFF"),
                        DarkerColour = Colour4.FromHex("#FFCC22"),
                        TextColour = Colour4.Black,
                        TextSize = 36,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Let's GO!",
                        Height = 80,
                        Action = () => actionFired = true,
                    };
                }
                else
                {
                    Child = button = new ShearedButton(200)
                    {
                        LighterColour = Colour4.FromHex("#FF86DD"),
                        DarkerColour = Colour4.FromHex("#DE31AE"),
                        TextColour = Colour4.White,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Press me",
                        Height = 80,
                        Action = () => actionFired = true,
                    };
                }
            });

            AddStep("set disabled", () => button.Enabled.Value = false);
            AddStep("press button", () => button.TriggerClick());
            AddAssert("action not fired", () => !actionFired);

            AddStep("set enabled", () => button.Enabled.Value = true);
            AddStep("press button", () => button.TriggerClick());
            AddAssert("action fired", () => actionFired);
        }

        [Test]
        public void TestShearedToggleButton()
        {
            ShearedToggleButton button = null;

            AddStep("create button", () =>
            {
                Child = button = new ShearedToggleButton(200)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "Toggle me",
                };
            });

            AddToggleStep("toggle button", active => button.Active.Value = active);
            AddToggleStep("toggle disabled", disabled => button.Active.Disabled = disabled);
        }

        [Test]
        public void TestSizing()
        {
            ShearedToggleButton toggleButton = null;

            AddStep("create fixed width button", () => Child = toggleButton = new ShearedToggleButton(200)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "Fixed width"
            });
            AddStep("change text", () => toggleButton.Text = "New text");

            AddStep("create auto-sizing button", () => Child = toggleButton = new ShearedToggleButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "This button autosizes to its text!"
            });
            AddStep("change text", () => toggleButton.Text = "New text");
        }

        [Test]
        public void TestDisabledState()
        {
            ShearedToggleButton button = null;

            AddStep("create button", () =>
            {
                Child = button = new ShearedToggleButton(200)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "Toggle me",
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
