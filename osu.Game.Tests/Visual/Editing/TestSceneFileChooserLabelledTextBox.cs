// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Screens.Edit.Setup;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneFileChooserLabelledTextBox : OsuManualInputManagerTestScene
    {
        private FileChooserLabelledTextBox fileChooser;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Container fileChooserContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };

            Child = new FillFlowContainer
            {
                Width = 600,
                AutoSizeAxes = Axes.Y,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    fileChooser = new FileChooserLabelledTextBox(".jpg", ".jpeg", ".png")
                    {
                        Label = "File Chooser",
                        FixedLabelWidth = 160,
                        PlaceholderText = "Click to select a file",
                        Target = fileChooserContainer,
                        TabbableContentContainer = this
                    },
                    fileChooserContainer
                }
            };
        });

        [Test]
        public void TestFileChooserToggles()
        {
            FileChooserLabelledTextBox.FileChooserOsuTextBox textBox = null;
            AddStep("retrieve text box", () => textBox = this.ChildrenOfType<FileChooserLabelledTextBox.FileChooserOsuTextBox>().First());

            AddStep("hover over text box", () => InputManager.MoveMouseTo(textBox));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("file chooser is expanded", () => fileChooser.IsExpanded);

            AddStep("hover over text box", () => InputManager.MoveMouseTo(textBox));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("file chooser is no longer expanded", () => !fileChooser.IsExpanded);
        }
    }
}
