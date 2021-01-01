// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class OverridableBindableTest
    {
        [Test]
        public void TestBaseValueChanges()
        {
            var setting = new OverridableBindable<double>(
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
            var setting = new OverridableBindable<float>(
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

        [Test]
        public void TestCustomExpectedParsing()
        {
            var setting = new OverridableBindable<float>(
                minValue: 2,
                maxValue: 8,
                defaultValue: 2);

            Assert.That(setting.HasCustomValue.Value, Is.False);
            Assert.That(setting.FinalValue.Value, Is.EqualTo(2));

            parseAndEnsureExpected(7f, 7f);
            parseAndEnsureExpected(new Bindable<float>(6f), 6f);
            parseAndEnsureExpected(new OverridableBindable<float>(5f) { HasCustomValue = { Value = true } }, 5f);

            parseAndEnsureExpected(14f, 8f);
            parseAndEnsureExpected(-5f, 2f);

            void parseAndEnsureExpected(object input, float expected)
            {
                setting.HasCustomValue.Value = false;
                setting.Parse(input);

                Assert.That(setting.HasCustomValue.Value, Is.True);
                Assert.That(setting.FinalValue.Value, Is.EqualTo(setting.CustomValue.Value));
                Assert.That(setting.FinalValue.Value, Is.EqualTo(expected));
            }
        }

        [Test]
        public void TestBaseExpectedParsing()
        {
            var setting = new OverridableBindable<float>(
                minValue: 0,
                maxValue: 10,
                defaultValue: 5);

            parseAndEnsureSame(new OverridableBindable<float>(9f));
            parseAndEnsureSame(null);

            void parseAndEnsureSame(object input)
            {
                setting.HasCustomValue.Value = true;
                setting.Parse(input);

                Assert.That(setting.HasCustomValue.Value, Is.False);
                Assert.That(setting.FinalValue.Value, Is.EqualTo(setting.BaseValue.Value));
                Assert.That(setting.FinalValue.Value, Is.EqualTo(5f));

                // custom value shouldn't be touched as well.
                Assert.That(setting.CustomValue.Value, Is.EqualTo(5f));
            }
        }

        [Test]
        public void TestSerialization()
        {
            var setting = new OverridableBindable<float>(
                minValue: 0,
                maxValue: 10,
                defaultValue: 5);

            setting.HasCustomValue.Value = true;

            Assert.That(setting.Serialize().Deserialize<float>(), Is.EqualTo(5f));

            var deserializedSetting = setting.Serialize().Deserialize<OverridableBindable<float>>();
            Assert.That(deserializedSetting.HasCustomValue.Value, Is.True);
            Assert.That(deserializedSetting.CustomValue.Value, Is.EqualTo(5f));

            setting.HasCustomValue.Value = false;

            Assert.That(setting.Serialize().Deserialize<float?>(), Is.Null);
        }
    }
}
