// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneSettingsItem : OsuTestScene
    {
        [Test]
        public void TestRestoreDefaultValueButtonVisibility()
        {
            SettingsTextBox textBox = null;
            RestoreDefaultValueButton<string> restoreDefaultValueButton = null;

            AddStep("create settings item", () =>
            {
                Child = textBox = new SettingsTextBox
                {
                    Current = new Bindable<string>
                    {
                        Default = "test",
                        Value = "test"
                    }
                };
            });
            AddUntilStep("wait for loaded", () => textBox.IsLoaded);
            AddStep("retrieve restore default button", () => restoreDefaultValueButton = textBox.ChildrenOfType<RestoreDefaultValueButton<string>>().Single());

            AddAssert("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);

            AddStep("change value from default", () => textBox.Current.Value = "non-default");
            AddUntilStep("restore button shown", () => restoreDefaultValueButton.Alpha > 0);

            AddStep("restore default", () => textBox.Current.SetDefault());
            AddUntilStep("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);
        }

        [Test]
        public void TestSetAndClearLabelText()
        {
            SettingsTextBox textBox = null;
            RestoreDefaultValueButton<string> restoreDefaultValueButton = null;
            OsuTextBox control = null;

            AddStep("create settings item", () =>
            {
                Child = textBox = new SettingsTextBox
                {
                    Current = new Bindable<string>
                    {
                        Default = "test",
                        Value = "test"
                    }
                };
            });
            AddUntilStep("wait for loaded", () => textBox.IsLoaded);
            AddStep("retrieve components", () =>
            {
                restoreDefaultValueButton = textBox.ChildrenOfType<RestoreDefaultValueButton<string>>().Single();
                control = textBox.ChildrenOfType<OsuTextBox>().Single();
            });

            AddStep("set non-default value", () => restoreDefaultValueButton.Current.Value = "non-default");
            AddAssert("default value button centre aligned to control size", () => Precision.AlmostEquals(restoreDefaultValueButton.Parent.DrawHeight, control.DrawHeight, 1));

            AddStep("set label", () => textBox.LabelText = "label text");
            AddAssert("default value button centre aligned to label size", () =>
            {
                var label = textBox.ChildrenOfType<OsuSpriteText>().Single(spriteText => spriteText.Text == "label text");
                return Precision.AlmostEquals(restoreDefaultValueButton.Parent.DrawHeight, label.DrawHeight, 1);
            });

            AddStep("clear label", () => textBox.LabelText = default);
            AddAssert("default value button centre aligned to control size", () => Precision.AlmostEquals(restoreDefaultValueButton.Parent.DrawHeight, control.DrawHeight, 1));

            AddStep("set warning text", () => textBox.WarningText = "This is some very important warning text! Hopefully it doesn't break the alignment of the default value indicator...");
            AddAssert("default value button centre aligned to control size", () => Precision.AlmostEquals(restoreDefaultValueButton.Parent.DrawHeight, control.DrawHeight, 1));
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
            RestoreDefaultValueButton<float> restoreDefaultValueButton = null;

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
            AddStep("retrieve restore default button", () => restoreDefaultValueButton = sliderBar.ChildrenOfType<RestoreDefaultValueButton<float>>().Single());

            AddAssert("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);

            AddStep("change value to next closest", () => sliderBar.Current.Value += current.Precision * 0.6f);
            AddUntilStep("restore button shown", () => restoreDefaultValueButton.Alpha > 0);

            AddStep("restore default", () => sliderBar.Current.SetDefault());
            AddUntilStep("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);
        }

        [Test]
        public void TestWarningTextVisibility()
        {
            SettingsNumberBox numberBox = null;

            AddStep("create settings item", () => Child = numberBox = new SettingsNumberBox());
            AddAssert("warning text not created", () => !numberBox.ChildrenOfType<SettingsNoticeText>().Any());

            AddStep("set warning text", () => numberBox.WarningText = "this is a warning!");
            AddAssert("warning text created", () => numberBox.ChildrenOfType<SettingsNoticeText>().Single().Alpha == 1);

            AddStep("unset warning text", () => numberBox.WarningText = default);
            AddAssert("warning text hidden", () => numberBox.ChildrenOfType<SettingsNoticeText>().Single().Alpha == 0);

            AddStep("set warning text again", () => numberBox.WarningText = "another warning!");
            AddAssert("warning text shown again", () => numberBox.ChildrenOfType<SettingsNoticeText>().Single().Alpha == 1);
        }
    }
}
