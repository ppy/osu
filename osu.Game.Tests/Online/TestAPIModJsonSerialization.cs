// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.Solo;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Tests.Online
{
    [TestFixture]
    public class TestAPIModJsonSerialization
    {
        [Test]
        public void TestAcronymIsPreserved()
        {
            var apiMod = new APIMod(new TestMod());

            var deserialized = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));

            Assert.That(deserialized?.Acronym, Is.EqualTo(apiMod.Acronym));
        }

        [Test]
        public void TestRawSettingIsPreserved()
        {
            var apiMod = new APIMod(new TestMod { TestSetting = { Value = 2 } });

            var deserialized = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));

            Assert.That(deserialized?.Settings, Contains.Key("test_setting").With.ContainValue(2.0));
        }

        [Test]
        public void TestConvertedModHasCorrectSetting()
        {
            var apiMod = new APIMod(new TestMod { TestSetting = { Value = 2 } });

            var deserialized = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));
            var converted = (TestMod)deserialized?.ToMod(new TestRuleset());

            Assert.That(converted?.TestSetting.Value, Is.EqualTo(2));
        }

        [Test]
        public void TestDeserialiseTimeRampMod()
        {
            // Create the mod with values different from default.
            var apiMod = new APIMod(new TestModTimeRamp
            {
                AdjustPitch = { Value = false },
                InitialRate = { Value = 1.25 },
                FinalRate = { Value = 0.25 }
            });

            var deserialised = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));
            var converted = (TestModTimeRamp)deserialised?.ToMod(new TestRuleset());

            Assert.That(converted, Is.Not.Null);

            Assert.That(converted.AdjustPitch.Value, Is.EqualTo(false));
            Assert.That(converted.InitialRate.Value, Is.EqualTo(1.25));
            Assert.That(converted.FinalRate.Value, Is.EqualTo(0.25));
        }

        [Test]
        public void TestDeserialiseDifficultyAdjustModWithExtendedLimits()
        {
            var apiMod = new APIMod(new TestModDifficultyAdjust
            {
                OverallDifficulty = { Value = 11 },
                ExtendedLimits = { Value = true }
            });

            var deserialised = JsonConvert.DeserializeObject<APIMod>(JsonConvert.SerializeObject(apiMod));
            var converted = (TestModDifficultyAdjust)deserialised?.ToMod(new TestRuleset());

            Assert.That(converted?.ExtendedLimits.Value, Is.True);
            Assert.That(converted?.OverallDifficulty.Value, Is.EqualTo(11));
        }

        [Test]
        public void TestDeserialiseSubmittableScoreWithEmptyMods()
        {
            var score = new SubmittableScore(new ScoreInfo { Ruleset = new OsuRuleset().RulesetInfo });

            var deserialised = JsonConvert.DeserializeObject<SubmittableScore>(JsonConvert.SerializeObject(score));

            Assert.That(deserialised?.Mods.Length, Is.Zero);
        }

        [Test]
        public void TestDeserialiseSubmittableScoreWithCustomModSetting()
        {
            var score = new SubmittableScore(new ScoreInfo
            {
                Ruleset = new OsuRuleset().RulesetInfo,
                Mods = new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 2 } } }
            });

            var deserialised = JsonConvert.DeserializeObject<SubmittableScore>(JsonConvert.SerializeObject(score));

            Assert.That((deserialised?.Mods[0])?.Settings["speed_change"], Is.EqualTo(2));
        }

        private class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => new Mod[]
            {
                new TestMod(),
                new TestModTimeRamp(),
                new TestModDifficultyAdjust()
            };

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => throw new NotImplementedException();

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();

            public override string Description { get; } = string.Empty;
            public override string ShortName { get; } = string.Empty;
        }

        private class TestMod : Mod
        {
            public override string Name => "Test Mod";
            public override string Acronym => "TM";
            public override string Description => "This is a test mod.";
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

        private class TestModTimeRamp : ModTimeRamp
        {
            public override string Name => "Test Mod";
            public override string Acronym => "TMTR";
            public override string Description => "This is a test mod.";
            public override double ScoreMultiplier => 1;

            [SettingSource("Initial rate", "The starting speed of the track")]
            public override BindableNumber<double> InitialRate { get; } = new BindableDouble
            {
                MinValue = 1,
                MaxValue = 2,
                Default = 1.5,
                Value = 1.5,
                Precision = 0.01,
            };

            [SettingSource("Final rate", "The speed increase to ramp towards")]
            public override BindableNumber<double> FinalRate { get; } = new BindableDouble
            {
                MinValue = 0,
                MaxValue = 1,
                Default = 0.5,
                Value = 0.5,
                Precision = 0.01,
            };

            [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
            public override BindableBool AdjustPitch { get; } = new BindableBool
            {
                Default = true,
                Value = true
            };
        }

        private class TestModDifficultyAdjust : ModDifficultyAdjust
        {
        }
    }
}
