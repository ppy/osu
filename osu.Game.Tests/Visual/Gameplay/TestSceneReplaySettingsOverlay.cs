// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneReplaySettingsOverlay : OsuTestScene
    {
        public TestSceneReplaySettingsOverlay()
        {
            ExampleContainer container;

            Add(new PlayerSettingsOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                State = { Value = Visibility.Visible }
            });

            Add(container = new ExampleContainer());

            AddStep(@"Add button", () => container.Add(new TriangleButton
            {
                RelativeSizeAxes = Axes.X,
                Text = @"Button",
            }));

            AddStep(@"Add checkbox", () => container.Add(new PlayerCheckbox
            {
                LabelText = "Checkbox",
            }));

            AddStep(@"Add textbox", () => container.Add(new FocusedTextBox
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                PlaceholderText = "Textbox",
                HoldFocus = false,
            }));
        }

        private class ExampleContainer : PlayerSettingsGroup
        {
            protected override string Title => @"example";
        }
    }
}
