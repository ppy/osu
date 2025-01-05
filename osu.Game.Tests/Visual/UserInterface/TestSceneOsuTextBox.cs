// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuTextBox : ThemeComparisonTestScene
    {
        private IEnumerable<OsuNumberBox> numberBoxes => this.ChildrenOfType<OsuNumberBox>();

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Padding = new MarginPadding(50f),
            Spacing = new Vector2(0f, 50f),
            Children = new[]
            {
                new OsuTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Normal textbox",
                },
                new OsuPasswordTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Password textbox",
                },
                new OsuNumberBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Number textbox"
                }
            }
        };

        [Test]
        public void TestNumberBox()
        {
            AddStep("create themed content", () => CreateThemedContent(OverlayColourScheme.Red));

            clearTextboxes(numberBoxes);
            AddStep("enter numbers", () => numberBoxes.ForEach(numberBox => numberBox.Text = "987654321"));
            expectedValue(numberBoxes, "987654321");

            clearTextboxes(numberBoxes);
            AddStep("enter text + single number", () => numberBoxes.ForEach(numberBox => numberBox.Text = "1 hello 2 world 3"));
            expectedValue(numberBoxes, "123");

            clearTextboxes(numberBoxes);
        }

        [Test]
        public void TestSelectAllOnFocus()
        {
            AddStep("create themed content", () => CreateThemedContent(OverlayColourScheme.Red));

            AddStep("enter numbers", () => numberBoxes.ForEach(numberBox => numberBox.Text = "987654321"));

            AddAssert("nothing selected", () => string.IsNullOrEmpty(numberBoxes.First().SelectedText));
            AddStep("click on a number box", () =>
            {
                InputManager.MoveMouseTo(numberBoxes.First());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("text selected", () => numberBoxes.First().SelectedText == "987654321");

            AddStep("click away", () =>
            {
                InputManager.MoveMouseTo(Vector2.Zero);
                InputManager.Click(MouseButton.Left);
            });

            Drawable textContainer = null!;

            AddStep("move mouse to end of text", () =>
            {
                textContainer = numberBoxes.First().ChildrenOfType<Container>().ElementAt(1);
                InputManager.MoveMouseTo(textContainer.ScreenSpaceDrawQuad.TopRight);
            });
            AddStep("hold mouse", () => InputManager.PressButton(MouseButton.Left));
            AddStep("drag to half", () => InputManager.MoveMouseTo(textContainer.ScreenSpaceDrawQuad.BottomRight - new Vector2(textContainer.ScreenSpaceDrawQuad.Width / 2 + 1f, 0)));
            AddStep("release mouse", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("half text selected", () => numberBoxes.First().SelectedText == "54321");
        }

        private void clearTextboxes(IEnumerable<OsuTextBox> textBoxes) => AddStep("clear textbox", () => textBoxes.ForEach(textBox => textBox.Text = null));
        private void expectedValue(IEnumerable<OsuTextBox> textBoxes, string value) => AddAssert("expected textbox value", () => textBoxes.All(textBox => textBox.Text == value));
    }
}
