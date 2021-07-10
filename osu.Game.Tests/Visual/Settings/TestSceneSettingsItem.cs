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
        /// Tests precision of the restore default value button, with a couple of single floating-point numbers that could potentially underflow.
        /// </summary>
        [TestCase(4.2f)]
        [TestCase(9.9f)]
        public void TestRestoreDefaultValueButtonPrecision(float initialValue)
        {
            SettingsSlider<float> sliderBar = null;
            RestoreDefaultValueButton<float> restoreDefaultValueButton = null;

            AddStep("create settings item", () =>
            {
                Child = sliderBar = new SettingsSlider<float>
                {
                    Current = new BindableFloat(initialValue)
                    {
                        MinValue = 0f,
                        MaxValue = 10f,
                        Precision = 0.1f,
                    }
                };

                restoreDefaultValueButton = sliderBar.ChildrenOfType<RestoreDefaultValueButton<float>>().Single();
            });

            AddAssert("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);

            AddStep("change value to 5.1f", () => sliderBar.Current.Value = 5.0f);
            AddUntilStep("restore button shown", () => restoreDefaultValueButton.Alpha > 0);

            AddStep("restore default", () => sliderBar.Current.SetDefault());
            AddUntilStep("restore button hidden", () => restoreDefaultValueButton.Alpha == 0);
        }
    }
}
