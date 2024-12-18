// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            var modDouble = new TestNonMatchingSettingTypeModDouble { TestSetting = { Value = setting_change } };
            var modBool = new TestNonMatchingSettingTypeModBool { TestSetting = { Default = false, Value = true } };
            var modInt = new TestNonMatchingSettingTypeModInt { TestSetting = { Value = (int)setting_change / 2 } };

            modDouble.CopyCommonSettingsFrom(modBool);
            modDouble.CopyCommonSettingsFrom(modInt);
            modBool.CopyCommonSettingsFrom(modDouble);
            modBool.CopyCommonSettingsFrom(modInt);
            modInt.CopyCommonSettingsFrom(modDouble);
            modInt.CopyCommonSettingsFrom(modBool);

            Assert.That(modDouble.TestSetting.Value, Is.EqualTo(setting_change));
            Assert.That(modBool.TestSetting.Value, Is.EqualTo(true));
            Assert.That(modInt.TestSetting.Value, Is.EqualTo((int)setting_change / 2));
        }

        [Test]
        public void TestDefaultValueKeptWhenCopied()
        {
            var modBoolTrue = new TestNonMatchingSettingTypeModBool { TestSetting = { Default = true, Value = false } };
            var modBoolFalse = new TestNonMatchingSettingTypeModBool { TestSetting = { Default = false, Value = true } };

            modBoolFalse.CopyCommonSettingsFrom(modBoolTrue);

            Assert.That(modBoolFalse.TestSetting.Default, Is.EqualTo(false));
            Assert.That(modBoolFalse.TestSetting.Value, Is.EqualTo(modBoolTrue.TestSetting.Value));
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
