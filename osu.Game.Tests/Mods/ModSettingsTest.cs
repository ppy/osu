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
        public void TestModSettingsUnboundWhenCopied()
        {
            var original = new OsuModDoubleTime();
            var copy = (OsuModDoubleTime)original.DeepClone();

            original.SpeedChange.Value = 2;

            Assert.That(original.SpeedChange.Value, Is.EqualTo(2.0));
            Assert.That(copy.SpeedChange.Value, Is.EqualTo(1.5));
        }

        [Test]
        public void TestMultiModSettingsUnboundWhenCopied()
        {
            var original = new MultiMod(new OsuModDoubleTime());
            var copy = (MultiMod)original.DeepClone();

            ((OsuModDoubleTime)original.Mods[0]).SpeedChange.Value = 2;

            Assert.That(((OsuModDoubleTime)original.Mods[0]).SpeedChange.Value, Is.EqualTo(2.0));
            Assert.That(((OsuModDoubleTime)copy.Mods[0]).SpeedChange.Value, Is.EqualTo(1.5));
        }

        [Test]
        public void TestCopySharedSettingsOfDifferentType()
        {
            const double setting_change = 2.5;

            var osuMod = new TestNonMatchinSettingTypeOsuMod();
            var maniaMod = new TestNonMatchinSettingTypeManiaMod();

            osuMod.TestSetting.Value = setting_change;
            maniaMod.CopySharedSettings(osuMod);
            osuMod.CopySharedSettings(maniaMod);

            Assert.That(maniaMod.TestSetting.IsDefault, "Value has been changed");
            Assert.That(osuMod.TestSetting.Value == setting_change);
        }

        private class TestNonMatchinSettingTypeOsuMod : TestNonMatchinSettingTypeMod
        {
            public override string Acronym => "NMO";
            public override BindableNumber<double> TestSetting { get; } = new BindableDouble(3.5);
        }

        private class TestNonMatchinSettingTypeManiaMod : TestNonMatchinSettingTypeMod
        {
            public override string Acronym => "NMM";
            public override Bindable<bool> TestSetting { get; } = new BindableBool(true);
        }

        private abstract class TestNonMatchinSettingTypeMod : Mod
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
