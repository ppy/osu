// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Online
{
    [TestFixture]
    public class TestAPIModSerialization
    {
        [Test]
        public void TestAcronymIsPreserved()
        {
            var apiMod = new APIMod(new TestMod());

            var deserialized = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));

            Assert.That(deserialized.Acronym, Is.EqualTo(apiMod.Acronym));
        }

        [Test]
        public void TestRawSettingIsPreserved()
        {
            var apiMod = new APIMod(new TestMod { TestSetting = { Value = 2 } });

            var deserialized = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));

            Assert.That(deserialized.Settings, Contains.Key("test_setting").With.ContainValue(2.0));
        }

        [Test]
        public void TestConvertedModHasCorrectSetting()
        {
            var apiMod = new APIMod(new TestMod { TestSetting = { Value = 2 } });

            var deserialized = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));
            var converted = (TestMod)deserialized.ToMod(new TestRuleset());

            Assert.That(converted.TestSetting.Value, Is.EqualTo(2));
        }

        [Test]
        public void TestDeserialiseTimeRampMod()
        {
            // Create the mod with values different from default.
            var apiMod = new APIMod(new TestCustomizableMod
            {
                BNumber1 = { Value = 1.25d },
                BNumber2 = { Value = -5.25f },
                CustomSetting = { CustomValue = { Value = 3f }, HasCustomValue = { Value = true } },
            });

            var deserialised = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));
            var converted = (TestCustomizableMod)deserialised.ToMod(new TestRuleset());

            Assert.That(converted.BNumber1.Value, Is.EqualTo(1.25d));
            Assert.That(converted.BNumber2.Value, Is.EqualTo(-5.25f));
            Assert.That(converted.DontChange.HasCustomValue.Value, Is.False);
            Assert.That(converted.CustomSetting.CustomValue.Value, Is.EqualTo(3f));
            Assert.That(converted.CustomSetting.HasCustomValue.Value, Is.True);
        }

        private class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => new Mod[]
            {
                new TestMod(),
                new TestCustomizableMod(),
            };

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => throw new System.NotImplementedException();

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new System.NotImplementedException();

            public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => throw new System.NotImplementedException();

            public override string Description { get; } = string.Empty;
            public override string ShortName { get; } = string.Empty;
        }

        private class TestMod : Mod
        {
            public override string Name => "Test Mod";
            public override string Acronym => "TM";
            public override double ScoreMultiplier => 1;

            [SettingSource("Test")]
            public BindableNumber<double> TestSetting { get; } = new BindableDouble
            {
                MinValue = 0,
                MaxValue = 10,
                Default = 5,
                Precision = 0.01,
            };
        }

        private class TestCustomizableMod : Mod
        {
            public override string Name => "Test Customizable Mod";
            public override string Acronym => "TCM";
            public override double ScoreMultiplier => 1;

            [SettingSource("Bindable number #1")]
            public BindableNumber<double> BNumber1 { get; } = new BindableDouble { MinValue = 1, MaxValue = 10 };

            [SettingSource("Bindable number #2")]
            public BindableNumber<float> BNumber2 { get; } = new BindableFloat { MinValue = -10, MaxValue = -1 };

            [SettingSource("Don't change this please")]
            public OverridableBindable<double> DontChange { get; } = new OverridableBindable<double>(6, 6, 10);

            [SettingSource("Custom difficulty setting")]
            public OverridableBindable<float> CustomSetting { get; } = new OverridableBindable<float>(0, -5, 5);
        }
    }
}
