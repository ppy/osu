// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tests.Mods
{
    [TestFixture]
    public partial class SettingSourceAttributeTest
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

        [Test]
        public void TestCustomControl()
        {
            var objectWithCustomSettingControl = new ClassWithCustomSettingControl();
            var settings = objectWithCustomSettingControl.CreateSettingsControls().ToArray();

            Assert.That(settings, Has.Length.EqualTo(1));
            Assert.That(settings[0], Is.TypeOf<CustomSettingsControl>());
        }

        private class ClassWithSettings
        {
            [SettingSource("Unordered setting", "Should be last")]
            public BindableFloat UnorderedSetting { get; } = new BindableFloat();

            [SettingSource("Second setting", "Another description", 2)]
            public BindableBool SecondSetting { get; } = new BindableBool();

            [SettingSource("First setting", "A description", 1)]
            public BindableDouble FirstSetting { get; } = new BindableDouble();

            [SettingSource("Third setting", "Yet another description", 3)]
            public BindableInt ThirdSetting { get; } = new BindableInt();
        }

        private class ClassWithCustomSettingControl
        {
            [SettingSource("Custom setting", "Should be a custom control", SettingControlType = typeof(CustomSettingsControl))]
            public BindableInt UnorderedSetting { get; } = new BindableInt();
        }

        private partial class CustomSettingsControl : SettingsItem<int>
        {
            protected override Drawable CreateControl() => new CustomControl();

            private partial class CustomControl : Drawable, IHasCurrentValue<int>
            {
                public Bindable<int> Current { get; set; } = new Bindable<int>();
            }
        }
    }
}
