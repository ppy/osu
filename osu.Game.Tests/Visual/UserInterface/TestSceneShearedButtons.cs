// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneShearedButtons : OsuManualInputManagerTestScene
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
            AddAssert("draw width is 200", () => toggleButton.DrawWidth, () => Is.EqualTo(200).Within(Precision.FLOAT_EPSILON));

            AddStep("change text", () => toggleButton.Text = "New text");
            AddAssert("draw width is 200", () => toggleButton.DrawWidth, () => Is.EqualTo(200).Within(Precision.FLOAT_EPSILON));

            AddStep("create auto-sizing button", () => Child = toggleButton = new ShearedToggleButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "This button autosizes to its text!"
            });
            AddAssert("button is wider than text", () => toggleButton.DrawWidth, () => Is.GreaterThan(toggleButton.ChildrenOfType<SpriteText>().Single().DrawWidth));

            float originalDrawWidth = 0;
            AddStep("store button width", () => originalDrawWidth = toggleButton.DrawWidth);

            AddStep("change text", () => toggleButton.Text = "New text");
            AddAssert("button is wider than text", () => toggleButton.DrawWidth, () => Is.GreaterThan(toggleButton.ChildrenOfType<SpriteText>().Single().DrawWidth));
            AddAssert("button width decreased", () => toggleButton.DrawWidth, () => Is.LessThan(originalDrawWidth));
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
