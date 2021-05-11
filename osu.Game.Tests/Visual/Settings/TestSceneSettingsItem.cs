// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneSettingsItem : OsuTestScene
    {
        [Test]
        public void TestRestoreDefaultValueButtonVisibility()
        {
            TestSettingsTextBox textBox = null;

            AddStep("create settings item", () => Child = textBox = new TestSettingsTextBox
            {
                Current = new Bindable<string>
                {
                    Default = "test",
                    Value = "test"
                }
            });
            AddAssert("restore button hidden", () => textBox.RestoreDefaultValueButton.Alpha == 0);

            AddStep("change value from default", () => textBox.Current.Value = "non-default");
            AddUntilStep("restore button shown", () => textBox.RestoreDefaultValueButton.Alpha > 0);

            AddStep("restore default", () => textBox.Current.SetDefault());
            AddUntilStep("restore button hidden", () => textBox.RestoreDefaultValueButton.Alpha == 0);
        }

        private class TestSettingsTextBox : SettingsTextBox
        {
            public new Drawable RestoreDefaultValueButton => this.ChildrenOfType<RestoreDefaultValueButton>().Single();
        }
    }
}
