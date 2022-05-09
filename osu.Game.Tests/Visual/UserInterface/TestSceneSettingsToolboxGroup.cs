// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneSettingsToolboxGroup : OsuManualInputManagerTestScene
    {
        public TestSceneSettingsToolboxGroup()
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

        [Test]
        public void TestClickExpandButtonMultipleTimes()
        {
            SettingsToolboxGroup group = null;

            AddAssert("group expanded by default", () => (group = this.ChildrenOfType<SettingsToolboxGroup>().First()).Expanded.Value);
            AddStep("click expand button multiple times", () =>
            {
                InputManager.MoveMouseTo(group.ChildrenOfType<IconButton>().Single());
                Scheduler.AddDelayed(() => InputManager.Click(MouseButton.Left), 100);
                Scheduler.AddDelayed(() => InputManager.Click(MouseButton.Left), 200);
                Scheduler.AddDelayed(() => InputManager.Click(MouseButton.Left), 300);
            });
            AddAssert("group contracted", () => !group.Expanded.Value);
        }

        private class ExampleContainer : PlayerSettingsGroup
        {
            public ExampleContainer()
                : base("example")
            {
            }
        }
    }
}
