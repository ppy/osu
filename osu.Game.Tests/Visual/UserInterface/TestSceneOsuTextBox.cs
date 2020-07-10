// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOsuTextBox : OsuTestScene
    {
        private readonly OsuNumberBox numberBox;

        public TestSceneOsuTextBox()
        {
            Child = new Container
            {
                Masking = true,
                CornerRadius = 10f,
                AutoSizeAxes = Axes.Both,
                Padding = new MarginPadding(15f),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.DarkSlateGray,
                        Alpha = 0.75f,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(50f),
                        Spacing = new Vector2(0f, 50f),
                        Children = new[]
                        {
                            new OsuTextBox
                            {
                                Width = 500f,
                                PlaceholderText = "Normal textbox",
                            },
                            new OsuPasswordTextBox
                            {
                                Width = 500f,
                                PlaceholderText = "Password textbox",
                            },
                            numberBox = new OsuNumberBox
                            {
                                Width = 500f,
                                PlaceholderText = "Number textbox"
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public void TestNumberBox()
        {
            clearTextbox(numberBox);
            AddStep("enter numbers", () => numberBox.Text = "987654321");
            expectedValue(numberBox, "987654321");

            clearTextbox(numberBox);
            AddStep("enter text + single number", () => numberBox.Text = "1 hello 2 world 3");
            expectedValue(numberBox, "123");

            clearTextbox(numberBox);
        }

        private void clearTextbox(OsuTextBox textBox) => AddStep("clear textbox", () => textBox.Text = null);
        private void expectedValue(OsuTextBox textBox, string value) => AddAssert("expected textbox value", () => textBox.Text == value);
    }
}
