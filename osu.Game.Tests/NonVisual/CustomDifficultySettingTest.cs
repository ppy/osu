// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class CustomDifficultySettingTest
    {
        [Test]
        public void TestBaseValueChanges()
        {
            var setting = new CustomDifficultySetting<double>(
                minValue: 1,
                maxValue: 11,
                defaultValue: 5);

            Assert.That(setting.FinalValue.Value, Is.EqualTo(5));
            Assert.That(setting.FinalValue.Disabled, Is.True);

            setting.BaseValue.Value = 7;

            Assert.That(setting.FinalValue.Value, Is.EqualTo(7));
            Assert.That(setting.FinalValue.Disabled, Is.True);
        }

        [Test]
        public void TestCustomValueSet()
        {
            var setting = new CustomDifficultySetting<float>(
                minValue: 5,
                maxValue: 15,
                defaultValue: 6);

            Assert.That(setting.FinalValue.Value, Is.EqualTo(6));
            Assert.That(setting.FinalValue.Disabled, Is.True);

            setting.CustomValue.Value = 9;
            setting.HasCustomValue.Value = true;

            Assert.That(setting.FinalValue.Value, Is.EqualTo(9));
            Assert.That(setting.FinalValue.Disabled, Is.False);

            // change base value to make sure nothing changes in final value.
            setting.BaseValue.Value = 11;

            Assert.That(setting.FinalValue.Value, Is.EqualTo(9));
            Assert.That(setting.FinalValue.Disabled, Is.False);

            // change custom value to ensure things are propagating.
            setting.CustomValue.Value = 6;

            Assert.That(setting.FinalValue.Value, Is.EqualTo(6));
            Assert.That(setting.FinalValue.Disabled, Is.False);

            // also change externally via FinalValue and ensure propagation inward.
            setting.FinalValue.Value = 8;

            Assert.That(setting.CustomValue.Value, Is.EqualTo(8));

            // finally, disable custom value to make sure we revert to base and disable changes.
            setting.HasCustomValue.Value = false;

            Assert.That(setting.FinalValue.Value, Is.EqualTo(11));
            Assert.That(setting.FinalValue.Disabled, Is.True);

            // re-enable once more for good measure.
            setting.HasCustomValue.Value = true;

            Assert.That(setting.FinalValue.Value, Is.EqualTo(8));
            Assert.That(setting.FinalValue.Disabled, Is.False);
        }
    }
}
