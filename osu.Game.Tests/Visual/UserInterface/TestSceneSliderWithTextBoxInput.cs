// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSliderWithTextBoxInput : OsuManualInputManagerTestScene
    {
        private SliderWithTextBoxInput<float> sliderWithTextBoxInput = null!;

        private OsuSliderBar<float> slider => sliderWithTextBoxInput.ChildrenOfType<OsuSliderBar<float>>().Single();
        private Nub nub => sliderWithTextBoxInput.ChildrenOfType<Nub>().Single();
        private OsuTextBox textBox => sliderWithTextBoxInput.ChildrenOfType<OsuTextBox>().Single();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create slider", () => Child = sliderWithTextBoxInput = new SliderWithTextBoxInput<float>("Test Slider")
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f,
                Current = new BindableFloat
                {
                    MinValue = -5,
                    MaxValue = 5,
                    Precision = 0.2f
                }
            });
        }

        [Test]
        public void TestNonInstantaneousMode()
        {
            AddStep("set instantaneous to false", () => sliderWithTextBoxInput.Instantaneous = false);

            AddStep("focus textbox", () => InputManager.ChangeFocus(textBox));
            AddStep("change text", () => textBox.Text = "3");
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.Zero);
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.Zero);

            AddStep("commit text", () => InputManager.Key(Key.Enter));
            AddAssert("slider moved", () => slider.Current.Value, () => Is.EqualTo(3));
            AddAssert("current changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(3));

            AddStep("move mouse to nub", () => InputManager.MoveMouseTo(nub));
            AddStep("hold left mouse", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move mouse to minimum", () => InputManager.MoveMouseTo(sliderWithTextBoxInput.ScreenSpaceDrawQuad.BottomLeft));
            AddAssert("textbox not changed", () => textBox.Current.Value, () => Is.EqualTo("3"));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(3));

            AddStep("release left mouse", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("textbox changed", () => textBox.Current.Value, () => Is.EqualTo("-5"));
            AddAssert("current changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("focus textbox", () => InputManager.ChangeFocus(textBox));
            AddStep("set text to invalid", () => textBox.Text = "garbage");
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(-5));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("commit text", () => InputManager.Key(Key.Enter));
            AddAssert("text restored", () => textBox.Text, () => Is.EqualTo("-5"));
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(-5));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("focus textbox", () => InputManager.ChangeFocus(textBox));
            AddStep("set text to invalid", () => textBox.Text = "garbage");
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(-5));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("lose focus", () => InputManager.ChangeFocus(null));
            AddAssert("text restored", () => textBox.Text, () => Is.EqualTo("-5"));
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(-5));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));
        }

        [Test]
        public void TestInstantaneousMode()
        {
            AddStep("set instantaneous to true", () => sliderWithTextBoxInput.Instantaneous = true);

            AddStep("focus textbox", () => InputManager.ChangeFocus(textBox));
            AddStep("change text", () => textBox.Text = "3");
            AddAssert("slider moved", () => slider.Current.Value, () => Is.EqualTo(3));
            AddAssert("current changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(3));

            AddStep("commit text", () => InputManager.Key(Key.Enter));
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(3));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(3));

            AddStep("move mouse to nub", () => InputManager.MoveMouseTo(nub));
            AddStep("hold left mouse", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move mouse to minimum", () => InputManager.MoveMouseTo(sliderWithTextBoxInput.ScreenSpaceDrawQuad.BottomLeft));
            AddAssert("textbox changed", () => textBox.Current.Value, () => Is.EqualTo("-5"));
            AddAssert("current changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("release left mouse", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("textbox not changed", () => textBox.Current.Value, () => Is.EqualTo("-5"));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("focus textbox", () => InputManager.ChangeFocus(textBox));
            AddStep("set text to invalid", () => textBox.Text = "garbage");
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(-5));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("commit text", () => InputManager.Key(Key.Enter));
            AddAssert("text restored", () => textBox.Text, () => Is.EqualTo("-5"));
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(-5));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("focus textbox", () => InputManager.ChangeFocus(textBox));
            AddStep("set text to invalid", () => textBox.Text = "garbage");
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(-5));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));

            AddStep("lose focus", () => InputManager.ChangeFocus(null));
            AddAssert("text restored", () => textBox.Text, () => Is.EqualTo("-5"));
            AddAssert("slider not moved", () => slider.Current.Value, () => Is.EqualTo(-5));
            AddAssert("current not changed", () => sliderWithTextBoxInput.Current.Value, () => Is.EqualTo(-5));
        }
    }
}
