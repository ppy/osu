// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Mods
{
    public class ModSettingsTest
    {
        [Test]
        public void TestModSettingsUnboundWhenCloned()
        {
            var original = new OsuModDoubleTime();
            var copy = (OsuModDoubleTime)original.DeepClone();

            original.SpeedChange.Value = 2;

            Assert.That(original.SpeedChange.Value, Is.EqualTo(2.0));
            Assert.That(copy.SpeedChange.Value, Is.EqualTo(1.5));
        }

        [Test]
        public void TestMultiModSettingsUnboundWhenCloned()
        {
            var original = new MultiMod(new OsuModDoubleTime());
            var copy = (MultiMod)original.DeepClone();

            ((OsuModDoubleTime)original.Mods[0]).SpeedChange.Value = 2;

            Assert.That(((OsuModDoubleTime)original.Mods[0]).SpeedChange.Value, Is.EqualTo(2.0));
            Assert.That(((OsuModDoubleTime)copy.Mods[0]).SpeedChange.Value, Is.EqualTo(1.5));
        }

        [Test]
        public void TestDifferentTypeSettingsKeptWhenCopied()
        {
            const double setting_change = 50.4;

            var modDouble = new TestNonMatchingSettingTypeModDouble();
            var modBool = new TestNonMatchingSettingTypeModBool();

            modDouble.TestSetting.Value = setting_change;
            modBool.TestSetting.Value = !modBool.TestSetting.Default;
            modDouble.CopySharedSettings(modBool);
            modBool.CopySharedSettings(modDouble);

            Assert.That(modDouble.TestSetting.Value, Is.EqualTo(setting_change));
            Assert.That(modBool.TestSetting.Value, Is.EqualTo(!modBool.TestSetting.Default));
        }

        [Test]
        public void TestDefaultValueKeptWhenCopied()
        {
            var modBoolTrue = new TestNonMatchingSettingTypeModBool { TestSetting = { Default = true, Value = false } };
            var modBoolFalse = new TestNonMatchingSettingTypeModBool { TestSetting = { Default = false, Value = true } };

            modBoolFalse.CopySharedSettings(modBoolTrue);

            Assert.That(modBoolFalse.TestSetting.Default, Is.EqualTo(false));
            Assert.That(modBoolFalse.TestSetting.Value, Is.EqualTo(modBoolTrue.TestSetting.Value));
        }

        [Test]
        public void TestValueResetsToDefaultWhenCopied()
        {
            var modDouble = new TestNonMatchingSettingTypeModDouble();
            var modInt = new TestNonMatchingSettingTypeModInt { TestSetting = { Value = 1 } };

            modInt.CopySharedSettings(modDouble);

            Assert.That(modInt.TestSetting.Value, Is.EqualTo(modInt.TestSetting.Default));
        }

        [Test]
        public void TestRelativelyScaleWithClampedRangeWhenCopied()
        {
            const double setting_change = 50.4;

            var modDouble100 = new TestNonMatchingSettingTypeModDouble { TestSetting = { MaxValue = 100, MinValue = 0, Value = setting_change } };
            var modDouble200 = new TestNonMatchingSettingTypeModDouble { TestSetting = { MaxValue = 200, MinValue = 0 } };

            modDouble200.CopySharedSettings(modDouble100);

            Assert.That(modDouble200.TestSetting.Value, Is.EqualTo(setting_change * 2));
        }

        [Test]
        public void TestCopyDoubleToIntWithDefaultRange()
        {
            const double setting_change = 50.4;

            var modDouble = new TestNonMatchingSettingTypeModDouble { TestSetting = { Value = setting_change } };
            var modInt = new TestNonMatchingSettingTypeModInt();

            modInt.CopySharedSettings(modDouble);

            Assert.That(modInt.TestSetting.Value, Is.EqualTo(Convert.ToInt32(setting_change)));
        }

        [Test]
        public void TestCopyDoubleToIntWithOutOfBoundsRange()
        {
            const double setting_change = 50.4;

            var modDouble = new TestNonMatchingSettingTypeModDouble { TestSetting = { MinValue = int.MinValue - 1d, Value = setting_change } };
            // make RangeConstrainedBindable.HasDefinedRange return true
            var modInt = new TestNonMatchingSettingTypeModInt { TestSetting = { MinValue = int.MinValue + 1 } };

            modInt.CopySharedSettings(modDouble);

            Assert.That(modInt.TestSetting.Value, Is.EqualTo(Convert.ToInt32(setting_change)));
        }

        [Test]
        public void TestCopyDoubleToIntWithOutOfBoundsValue()
        {
            var modDouble = new TestNonMatchingSettingTypeModDouble { TestSetting = { MinValue = int.MinValue + 1, Value = int.MaxValue + 1d } };
            var modInt = new TestNonMatchingSettingTypeModInt { TestSetting = { MinValue = int.MinValue + 1 } };

            modInt.CopySharedSettings(modDouble);

            Assert.That(modInt.TestSetting.Value, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void TestCopyIntToDoubleWithDefaultRange()
        {
            const int setting_change = 50;

            var modInt = new TestNonMatchingSettingTypeModInt { TestSetting = { Value = setting_change } };
            var modDouble = new TestNonMatchingSettingTypeModDouble();

            modDouble.CopySharedSettings(modInt);

            Assert.That(modDouble.TestSetting.Value, Is.EqualTo(setting_change));
        }

        private class TestNonMatchingSettingTypeModDouble : TestNonMatchingSettingTypeMod
        {
            public override string Acronym => "NMD";
            public override BindableNumber<double> TestSetting { get; } = new BindableDouble();
        }

        private class TestNonMatchingSettingTypeModInt : TestNonMatchingSettingTypeMod
        {
            public override string Acronym => "NMI";
            public override BindableNumber<int> TestSetting { get; } = new BindableInt();
        }

        private class TestNonMatchingSettingTypeModBool : TestNonMatchingSettingTypeMod
        {
            public override string Acronym => "NMB";
            public override Bindable<bool> TestSetting { get; } = new BindableBool();
        }

        private abstract class TestNonMatchingSettingTypeMod : Mod
        {
            public override string Name => "Non-matching setting type mod";
            public override LocalisableString Description => "Description";
            public override double ScoreMultiplier => 1;
            public override ModType Type => ModType.Conversion;

            [SettingSource("Test setting")]
            public abstract IBindable TestSetting { get; }
        }
    }
}
