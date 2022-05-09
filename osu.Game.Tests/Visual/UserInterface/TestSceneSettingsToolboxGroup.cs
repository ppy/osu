// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneSettingsToolboxGroup : OsuManualInputManagerTestScene
    {
        private SettingsToolboxGroup group;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = group = new SettingsToolboxGroup("example")
            {
                Children = new Drawable[]
                {
                    new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = @"Button",
                        Enabled = { Value = true },
                    },
                    new OsuCheckbox
                    {
                        LabelText = @"Checkbox",
                    },
                    new OutlinedTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        PlaceholderText = @"Textbox",
                    }
                },
            };
        });

        [Test]
        public void TestClickExpandButtonMultipleTimes()
        {
            AddAssert("group expanded by default", () => group.Expanded.Value);
            AddStep("click expand button multiple times", () =>
            {
                InputManager.MoveMouseTo(group.ChildrenOfType<IconButton>().Single());
                Scheduler.AddDelayed(() => InputManager.Click(MouseButton.Left), 100);
                Scheduler.AddDelayed(() => InputManager.Click(MouseButton.Left), 200);
                Scheduler.AddDelayed(() => InputManager.Click(MouseButton.Left), 300);
            });
            AddAssert("group contracted", () => !group.Expanded.Value);
        }
    }
}
