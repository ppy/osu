// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSearchTextBox : OsuTestScene
    {
        private SearchTextBox textBox = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = textBox = new SearchTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 400,
                Scale = new Vector2(2f),
                HoldFocus = true,
            };
        });

        [Test]
        public void TestSelectionOnFocus()
        {
            AddStep("set text", () => textBox.Text = "some text");
            AddAssert("no text selected", () => textBox.SelectedText == string.Empty);
            AddStep("hide text box", () => textBox.Hide());
            AddStep("show text box", () => textBox.Show());
            AddAssert("search text selected", () => textBox.SelectedText == textBox.Text);
        }
    }
}
