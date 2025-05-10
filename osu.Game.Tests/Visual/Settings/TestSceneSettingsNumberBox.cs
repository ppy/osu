// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneSettingsNumberBox : OsuTestScene
    {
        private SettingsNumberBox numberBox;
        private OsuTextBox textBox;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create number box", () => Child = numberBox = new SettingsNumberBox());
            AddStep("get inner text box", () => textBox = numberBox.ChildrenOfType<OsuTextBox>().Single());
        }

        [Test]
        public void TestLargeInteger()
        {
            AddStep("set current to 1,000,000,000", () => numberBox.Current.Value = 1_000_000_000);
            AddAssert("text box text is correct", () => textBox.Text == "1000000000");
        }

        [Test]
        public void TestUserInput()
        {
            inputText("42");
            currentValueIs(42);
            currentTextIs("42");

            inputText(string.Empty);
            currentValueIs(null);
            currentTextIs(string.Empty);

            inputText("555");
            currentValueIs(555);
            currentTextIs("555");

            inputText("-4444");
            // attempting to input the minus will raise an input error, the rest will pass through fine.
            currentValueIs(4444);
            currentTextIs("4444");

            // checking the upper bound.
            inputText(int.MaxValue.ToString());
            currentValueIs(int.MaxValue);
            currentTextIs(int.MaxValue.ToString());

            inputText(smallestOverflowValue.ToString());
            currentValueIs(int.MaxValue);
            currentTextIs(int.MaxValue.ToString());

            inputText("0");
            currentValueIs(0);
            currentTextIs("0");

            // checking that leading zeroes are stripped.
            inputText("00");
            currentValueIs(0);
            currentTextIs("0");

            inputText("01");
            currentValueIs(1);
            currentTextIs("1");
        }

        private void inputText(string text) => AddStep($"set textbox text to {text}", () => textBox.Text = text);
        private void currentValueIs(int? value) => AddAssert($"current value is {value?.ToString() ?? "null"}", () => numberBox.Current.Value == value);
        private void currentTextIs(string value) => AddAssert($"current text is {value}", () => textBox.Text == value);

        /// <summary>
        /// The smallest number that overflows <see langword="int"/>.
        /// </summary>
        private static long smallestOverflowValue => 1L + int.MaxValue;
    }
}
