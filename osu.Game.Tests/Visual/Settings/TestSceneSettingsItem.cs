// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
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

                restoreDefaultValueButton = textBox.ChildrenOfType<RestoreDefaultValueButton<string>>().Single();
            });
            AddAssert("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);

            AddStep("change value from default", () => textBox.Current.Value = "non-default");
            AddUntilStep("restore button shown", () => restoreDefaultValueButton.Alpha > 0);

            AddStep("restore default", () => textBox.Current.SetDefault());
            AddUntilStep("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);
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

                restoreDefaultValueButton = sliderBar.ChildrenOfType<RestoreDefaultValueButton<float>>().Single();
            });

            AddAssert("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);

            AddStep("change value to next closest", () => sliderBar.Current.Value += current.Precision * 0.6f);
            AddUntilStep("restore button shown", () => restoreDefaultValueButton.Alpha > 0);

            AddStep("restore default", () => sliderBar.Current.SetDefault());
            AddUntilStep("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);
        }
    }
}
