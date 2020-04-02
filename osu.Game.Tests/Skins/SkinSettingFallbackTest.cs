// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Skinning;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    public class SkinSettingFallbackTest
    {
        private readonly ISkin skin = new TestSkin();

        /// <summary>
        /// Tests behaviour of looking for value of a setting (<see cref="TestConfiguration.ValueFBSetting"/>) that has a fallback "default" value.
        /// </summary>
        [Test]
        public void TestSettingWithDefaultValue()
        {
            var configSetting = skin.GetConfigOrDefault<TestConfiguration, string>(TestConfiguration.ValueFBSetting);

            Assert.AreEqual("default value", configSetting.Value);
        }

        /// <summary>
        /// Tests behaviour of looking for value of a setting (<see cref="TestConfiguration.SettingFBSetting"/>)
        /// that falls back to another setting (<see cref="FallbackConfiguration.NormalSetting"/>).
        /// </summary>
        [Test]
        public void TestSettingWithFallbackToAnotherSetting()
        {
            var configSetting = skin.GetConfigOrDefault<TestConfiguration, string>(TestConfiguration.SettingFBSetting);
            var fallbackSetting = skin.GetConfigOrDefault<FallbackConfiguration, string>(FallbackConfiguration.NormalSetting);

            Assert.AreEqual("value", configSetting.Value);
            Assert.AreEqual(fallbackSetting.Value, configSetting.Value);
        }

        /// <summary>
        /// Tests behaviour of looking for value of a setting (<see cref="TestConfiguration.ValueFBSettingFBSetting"/>)
        /// that falls back to another setting (<see cref="FallbackConfiguration.SettingFBFallbackSetting"/>)
        /// that has a fallback "default" value.
        /// </summary>
        [Test]
        public void TestSettingWithFallbackToAnotherSettingWithDefaultValue()
        {
            var configSetting = skin.GetConfigOrDefault<TestConfiguration, string>(TestConfiguration.ValueFBSettingFBSetting);
            var fallbackSetting = skin.GetConfigOrDefault<FallbackConfiguration, string>(FallbackConfiguration.ValueFBFallbackSetting);

            Assert.AreEqual("default value from fallback setting", configSetting.Value);
            Assert.AreEqual(fallbackSetting.Value, configSetting.Value);
        }

        /// <summary>
        /// Tests behaviour of looking for value of a setting (<see cref="TestConfiguration.SettingFBSettingFBSetting"/>)
        /// that falls back to another setting (<see cref="FallbackConfiguration.SettingFBFallbackSetting"/>.
        /// </summary>
        [Test]
        public void TestSettingWithDoubleFallbackToAnotherSettings()
        {
            var configSetting = skin.GetConfigOrDefault<TestConfiguration, string>(TestConfiguration.SettingFBSettingFBSetting);
            var fallbackSetting = skin.GetConfigOrDefault<FallbackConfiguration, string>(FallbackConfiguration.SettingFBFallbackSetting);
            var fallbackSetting2 = skin.GetConfigOrDefault<FallbackConfiguration, string>(FallbackConfiguration.NormalSetting);

            Assert.AreEqual("value", configSetting.Value);
            Assert.AreEqual(fallbackSetting.Value, configSetting.Value);
            Assert.AreEqual(fallbackSetting2.Value, configSetting.Value);
        }

        /// <summary>
        /// Tests behaviour of looking for value of a setting (<see cref="TestConfiguration.ValueFBExistingSetting"/>)
        /// that has a fallback "default" value but has an existing value assigned to it.
        /// </summary>
        [Test]
        public void TestSettingWithDefaultValueButExisting()
        {
            var configSetting = skin.GetConfigOrDefault<TestConfiguration, string>(TestConfiguration.ValueFBExistingSetting);
            Assert.AreEqual("existing value", configSetting.Value);
        }

        /// <summary>
        /// Tests behaviour of looking for value of a setting (<see cref="TestConfiguration.SettingFBExistingSetting"/>)
        /// that falls back to another setting (<see cref="FallbackConfiguration.NormalSetting"/>) but has an existing value assigned to it.
        /// </summary>
        [Test]
        public void TestSettingWithFallbackToAnotherSettingButExisting()
        {
            var configSetting = skin.GetConfigOrDefault<TestConfiguration, string>(TestConfiguration.SettingFBExistingSetting);
            var fallbackSetting = skin.GetConfigOrDefault<FallbackConfiguration, string>(FallbackConfiguration.NormalSetting);

            Assert.AreEqual("existing value", configSetting.Value);
            Assert.AreNotEqual(fallbackSetting.Value, configSetting.Value);
        }

        private class TestSkin : LegacySkin
        {
            public TestSkin()
                : base(new SkinInfo(), null, null, string.Empty)
            {
                Configuration.ConfigDictionary[nameof(TestConfiguration.ValueFBExistingSetting)] = "existing value";
                Configuration.ConfigDictionary[nameof(TestConfiguration.SettingFBExistingSetting)] = "existing value";

                Configuration.ConfigDictionary[nameof(FallbackConfiguration.NormalSetting)] = "value";
            }
        }

        private enum TestConfiguration
        {
            [SkinSettingFallback(SkinSettingFallbackType.Value, "default value")]
            ValueFBSetting,

            [SkinSettingFallback(SkinSettingFallbackType.Setting, FallbackConfiguration.NormalSetting)]
            SettingFBSetting,

            [SkinSettingFallback(SkinSettingFallbackType.Setting, FallbackConfiguration.ValueFBFallbackSetting)]
            ValueFBSettingFBSetting,

            [SkinSettingFallback(SkinSettingFallbackType.Setting, FallbackConfiguration.SettingFBFallbackSetting)]
            SettingFBSettingFBSetting,

            [SkinSettingFallback(SkinSettingFallbackType.Value, "default value")]
            ValueFBExistingSetting,

            [SkinSettingFallback(SkinSettingFallbackType.Setting, FallbackConfiguration.NormalSetting)]
            SettingFBExistingSetting,
        }

        private enum FallbackConfiguration
        {
            NormalSetting,

            [SkinSettingFallback(SkinSettingFallbackType.Value, "default value from fallback setting")]
            ValueFBFallbackSetting,

            [SkinSettingFallback(SkinSettingFallbackType.Setting, NormalSetting)]
            SettingFBFallbackSetting,
        }
    }
}
