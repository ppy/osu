// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public partial class TestSceneSettingsItem : OsuTestScene
    {
        [Test]
        public void TestRestoreDefaultValueButtonVisibility()
        {
            SettingsTextBox textBox = null;
            RevertToDefaultButton<string> revertToDefaultButton = null;

            AddStep("create settings item", () =>
            {
                Child = textBox = new SettingsTextBox
                {
                    Current = new Bindable<string>("test")
                };
            });
            AddUntilStep("wait for loaded", () => textBox.IsLoaded);
            AddStep("retrieve restore default button", () => revertToDefaultButton = textBox.ChildrenOfType<RevertToDefaultButton<string>>().Single());

            AddAssert("restore button hidden", () => revertToDefaultButton.Alpha == 0);

            AddStep("change value from default", () => textBox.Current.Value = "non-default");
            AddUntilStep("restore button shown", () => revertToDefaultButton.Alpha > 0);

            AddStep("disable setting", () => textBox.Current.Disabled = true);
            AddUntilStep("restore button still shown", () => revertToDefaultButton.Alpha > 0);

            AddStep("enable setting", () => textBox.Current.Disabled = false);
            AddStep("restore default", () => textBox.Current.SetDefault());
            AddUntilStep("restore button hidden", () => revertToDefaultButton.Alpha == 0);

            AddStep("disable setting", () => textBox.Current.Disabled = true);
            AddUntilStep("restore button still hidden", () => revertToDefaultButton.Alpha == 0);
        }

        [Test]
        public void TestSetAndClearLabelText()
        {
            SettingsTextBox textBox = null;
            RevertToDefaultButton<string> revertToDefaultButton = null;
            OsuTextBox control = null;

            AddStep("create settings item", () =>
            {
                Child = textBox = new SettingsTextBox
                {
                    Current = new Bindable<string>("test")
                };
            });
            AddUntilStep("wait for loaded", () => textBox.IsLoaded);
            AddStep("retrieve components", () =>
            {
                revertToDefaultButton = textBox.ChildrenOfType<RevertToDefaultButton<string>>().Single();
                control = textBox.ChildrenOfType<OsuTextBox>().Single();
            });

            AddStep("set non-default value", () => revertToDefaultButton.Current.Value = "non-default");
            AddAssert("default value button centre aligned to control size", () => Precision.AlmostEquals(revertToDefaultButton.Parent!.DrawHeight, control.DrawHeight, 1));

            AddStep("set label", () => textBox.LabelText = "label text");
            AddAssert("default value button centre aligned to label size", () =>
            {
                var label = textBox.ChildrenOfType<OsuSpriteText>().Single(spriteText => spriteText.Text == "label text");
                return Precision.AlmostEquals(revertToDefaultButton.Parent!.DrawHeight, label.DrawHeight, 1);
            });

            AddStep("clear label", () => textBox.LabelText = default);
            AddAssert("default value button centre aligned to control size", () => Precision.AlmostEquals(revertToDefaultButton.Parent!.DrawHeight, control.DrawHeight, 1));

            AddStep("set warning text", () => textBox.SetNoticeText("This is some very important warning text! Hopefully it doesn't break the alignment of the default value indicator...", true));
            AddAssert("default value button centre aligned to control size", () => Precision.AlmostEquals(revertToDefaultButton.Parent!.DrawHeight, control.DrawHeight, 1));
        }

        /// <summary>
        /// Ensures that the reset to default button uses the correct implementation of IsDefault to determine whether it should be shown or not.
        /// Values have been chosen so that after being set, Value != Default (but they are close enough that the difference is negligible compared to Precision).
        /// </summary>
        [TestCase(4.2f)]
        [TestCase(9.9f)]
        public void TestRestoreDefaultValueButtonPrecision(float initialValue)
        {
            BindableFloat current = null;
            SettingsSlider<float> sliderBar = null;
            RevertToDefaultButton<float> revertToDefaultButton = null;

            AddStep("create settings item", () =>
            {
                Child = sliderBar = new SettingsSlider<float>
                {
                    Current = current = new BindableFloat(initialValue)
                    {
                        MinValue = 0f,
                        MaxValue = 10f,
                        Precision = 0.1f,
                    }
                };
            });
            AddUntilStep("wait for loaded", () => sliderBar.IsLoaded);
            AddStep("retrieve restore default button", () => revertToDefaultButton = sliderBar.ChildrenOfType<RevertToDefaultButton<float>>().Single());

            AddAssert("restore button hidden", () => revertToDefaultButton.Alpha == 0);

            AddStep("change value to next closest", () => sliderBar.Current.Value += current.Precision * 0.6f);
            AddUntilStep("restore button shown", () => revertToDefaultButton.Alpha > 0);

            AddStep("restore default", () => sliderBar.Current.SetDefault());
            AddUntilStep("restore button hidden", () => revertToDefaultButton.Alpha == 0);
        }

        [Test]
        public void TestWarningTextVisibility()
        {
            SettingsNumberBox numberBox = null;

            AddStep("create settings item", () => Child = numberBox = new SettingsNumberBox());
            AddAssert("warning text not created", () => !numberBox.ChildrenOfType<LinkFlowContainer>().Any());

            AddStep("set warning text", () => numberBox.SetNoticeText("this is a warning!", true));
            AddAssert("warning text created", () => numberBox.ChildrenOfType<LinkFlowContainer>().Single().Alpha == 1);

            AddStep("unset warning text", () => numberBox.ClearNoticeText());
            AddAssert("warning text hidden", () => !numberBox.ChildrenOfType<LinkFlowContainer>().Any());

            AddStep("set warning text again", () => numberBox.SetNoticeText("another warning!", true));
            AddAssert("warning text shown again", () => numberBox.ChildrenOfType<LinkFlowContainer>().Single().Alpha == 1);

            AddStep("set non warning text", () => numberBox.SetNoticeText("you did good!"));
        }
    }
}
