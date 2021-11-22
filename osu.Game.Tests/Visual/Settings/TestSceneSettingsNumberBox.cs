// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tests.Visual.Settings
{
    public class TestSceneSettingsNumberBox : OsuTestScene
    {
        [Test]
        public void TestLargeInteger()
        {
            SettingsNumberBox numberBox = null;

            AddStep("create number box", () => Child = numberBox = new SettingsNumberBox());

            AddStep("set value to 1,000,000,000", () => numberBox.Current.Value = 1_000_000_000);
            AddAssert("text box text is correct", () => numberBox.ChildrenOfType<OsuTextBox>().Single().Current.Value == "1000000000");
        }
    }
}
