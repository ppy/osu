// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using NUnit.Framework;
using osuTK;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSettingsButton : OsuManualInputManagerTestScene
    {
        private readonly SettingsButton settingsButton;

        public TestSceneSettingsButton()
        {
            Add(new Container
            {
                AutoSizeAxes = Axes.Y,
                Width = 500,
                Child = settingsButton = new SettingsButton
                {
                    Enabled = { Value = true },
                    Text = "Test settings button"
                }
            });
        }

        [Test]
        public void TestInputAtPaddedArea()
        {
            AddStep("Move cursor to button", () => InputManager.MoveMouseTo(settingsButton));
            AddAssert("Button is hovered", () => settingsButton.IsHovered);
            AddStep("Move cursor to padded area", () => InputManager.MoveMouseTo(settingsButton.ScreenSpaceDrawQuad.TopLeft + new Vector2(SettingsPanel.CONTENT_MARGINS / 2f, 10)));
            AddAssert("Cursor within a button", () => settingsButton.ScreenSpaceDrawQuad.Contains(InputManager.CurrentState.Mouse.Position));
            AddAssert("Button is not hovered", () => !settingsButton.IsHovered);
        }
    }
}
