// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Tests.Mods
{
    [TestFixture]
    public class SettingsSourceAttributeTest
    {
        [Test]
        public void TestOrdering()
        {
            var objectWithSettings = new ClassWithSettings();

            var orderedSettings = objectWithSettings.GetOrderedSettingsSourceProperties().ToArray();

            Assert.That(orderedSettings, Has.Length.EqualTo(4));

            Assert.That(orderedSettings[0].Item2.Name, Is.EqualTo(nameof(ClassWithSettings.FirstSetting)));
            Assert.That(orderedSettings[1].Item2.Name, Is.EqualTo(nameof(ClassWithSettings.SecondSetting)));
            Assert.That(orderedSettings[2].Item2.Name, Is.EqualTo(nameof(ClassWithSettings.ThirdSetting)));
            Assert.That(orderedSettings[3].Item2.Name, Is.EqualTo(nameof(ClassWithSettings.UnorderedSetting)));
        }

        private class ClassWithSettings
        {
            [SettingSource("Unordered setting", "Should be last")]
            public BindableFloat UnorderedSetting { get; set; } = new BindableFloat();

            [SettingSource("Second setting", "Another description", 2)]
            public BindableBool SecondSetting { get; set; } = new BindableBool();

            [SettingSource("First setting", "A description", 1)]
            public BindableDouble FirstSetting { get; set; } = new BindableDouble();

            [SettingSource("Third setting", "Yet another description", 3)]
            public BindableInt ThirdSetting { get; set; } = new BindableInt();
        }
    }
}
