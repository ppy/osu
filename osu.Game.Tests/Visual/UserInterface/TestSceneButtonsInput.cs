// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using NUnit.Framework;
using osuTK;
using osu.Game.Overlays;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneButtonsInput : OsuManualInputManagerTestScene
    {
        private const int width = 500;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        private readonly SettingsButton settingsButton;
        private readonly OsuClickableContainer clickableContainer;
        private readonly RoundedButton roundedButton;
        private readonly ShearedButton shearedButton;

        public TestSceneButtonsInput()
        {
            Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                Width = 500,
                Spacing = new Vector2(0, 5),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    clickableContainer = new OsuClickableContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Enabled = { Value = true },
                        Masking = true,
                        CornerRadius = 20,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Red
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Rounded clickable container"
                            }
                        }
                    },
                    settingsButton = new SettingsButton
                    {
                        Enabled = { Value = true },
                        Text = "Settings button"
                    },
                    roundedButton = new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Enabled = { Value = true },
                        Text = "Rounded button"
                    },
                    shearedButton = new ShearedButton(width)
                    {
                        Text = "Sheared button",
                        LighterColour = Colour4.FromHex("#FFFFFF"),
                        DarkerColour = Colour4.FromHex("#FFCC22"),
                        TextColour = Colour4.Black,
                        Height = 40,
                        Enabled = { Value = true },
                        Padding = new MarginPadding(0)
                    }
                }
            });
        }

        [Test]
        public void TestSettingsButtonInput()
        {
            AddStep("Move cursor to button", () => InputManager.MoveMouseTo(settingsButton));
            AddAssert("Button is hovered", () => settingsButton.IsHovered);
            AddStep("Move cursor to padded area", () => InputManager.MoveMouseTo(settingsButton.ScreenSpaceDrawQuad.TopLeft + new Vector2(SettingsPanel.CONTENT_MARGINS / 2f, 10)));
            AddAssert("Cursor within a button", () => settingsButton.ScreenSpaceDrawQuad.Contains(InputManager.CurrentState.Mouse.Position));
            AddAssert("Button is not hovered", () => !settingsButton.IsHovered);
        }

        [Test]
        public void TestRoundedButtonInput()
        {
            AddStep("Move cursor to button", () => InputManager.MoveMouseTo(roundedButton));
            AddAssert("Button is hovered", () => roundedButton.IsHovered);
            AddStep("Move cursor to corner", () => InputManager.MoveMouseTo(roundedButton.ScreenSpaceDrawQuad.TopLeft + Vector2.One));
            AddAssert("Cursor within a button", () => roundedButton.ScreenSpaceDrawQuad.Contains(InputManager.CurrentState.Mouse.Position));
            AddAssert("Button is not hovered", () => !roundedButton.IsHovered);
        }

        [Test]
        public void TestShearedButtonInput()
        {
            AddStep("Move cursor to button", () => InputManager.MoveMouseTo(shearedButton));
            AddAssert("Button is hovered", () => shearedButton.IsHovered);
            AddStep("Move cursor to corner", () => InputManager.MoveMouseTo(shearedButton.ScreenSpaceDrawQuad.TopLeft + Vector2.One));
            AddAssert("Cursor within a button", () => shearedButton.ScreenSpaceDrawQuad.Contains(InputManager.CurrentState.Mouse.Position));
            AddAssert("Button is not hovered", () => !shearedButton.IsHovered);
        }

        [Test]
        public void TestRoundedClickableContainerInput()
        {
            AddStep("Move cursor to button", () => InputManager.MoveMouseTo(clickableContainer));
            AddAssert("Button is hovered", () => clickableContainer.IsHovered);
            AddStep("Move cursor to corner", () => InputManager.MoveMouseTo(clickableContainer.ScreenSpaceDrawQuad.TopLeft + Vector2.One));
            AddAssert("Cursor within a button", () => clickableContainer.ScreenSpaceDrawQuad.Contains(InputManager.CurrentState.Mouse.Position));
            AddAssert("Button is not hovered", () => !clickableContainer.IsHovered);
        }
    }
}
