// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.IO;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    [HeadlessTest]
    public partial class TestSceneSkinConfigurationLookup : OsuTestScene
    {
        private UserSkinSource userSource;
        private BeatmapSkinSource beatmapSource;
        private SkinRequester requester;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Add(new SkinProvidingContainer(userSource = new UserSkinSource())
                .WithChild(new SkinProvidingContainer(beatmapSource = new BeatmapSkinSource())
                    .WithChild(requester = new SkinRequester())));
        });

        [Test]
        public void TestBasicLookup()
        {
            AddStep("Add config values", () =>
            {
                userSource.Configuration.ConfigDictionary["Lookup"] = "user skin";
                beatmapSource.Configuration.ConfigDictionary["Lookup"] = "beatmap skin";
            });

            AddAssert("Check lookup finds beatmap skin", () => requester.GetConfig<string, string>("Lookup")?.Value == "beatmap skin");
        }

        [Test]
        public void TestFloatLookup()
        {
            AddStep("Add config values", () => userSource.Configuration.ConfigDictionary["FloatTest"] = "1.1");
            AddAssert("Check float parse lookup", () => requester.GetConfig<string, float>("FloatTest")?.Value == 1.1f);
        }

        [TestCase("0", false)]
        [TestCase("1", true)]
        [TestCase("2", true)] // https://github.com/ppy/osu/issues/18579
        public void TestBoolLookup(string originalValue, bool expectedParsedValue)
        {
            AddStep("Add config values", () => userSource.Configuration.ConfigDictionary["BoolTest"] = originalValue);
            AddAssert("Check bool parse lookup", () => requester.GetConfig<string, bool>("BoolTest")?.Value == expectedParsedValue);
        }

        [Test]
        public void TestEnumLookup()
        {
            AddStep("Add config values", () => userSource.Configuration.ConfigDictionary["Test"] = "Test2");
            AddAssert("Check enum parse lookup", () => requester.GetConfig<LookupType, ValueType>(LookupType.Test)?.Value == ValueType.Test2);
        }

        [Test]
        public void TestLookupFailure()
        {
            AddAssert("Check lookup failure", () => requester.GetConfig<string, float>("Lookup") == null);
        }

        [Test]
        public void TestLookupNull()
        {
            AddStep("Add config values", () => userSource.Configuration.ConfigDictionary["Lookup"] = null);

            AddAssert("Check lookup null", () =>
            {
                var bindable = requester.GetConfig<string, string>("Lookup");
                return bindable != null && bindable.Value == null;
            });
        }

        [Test]
        public void TestColourLookup()
        {
            AddStep("Add config colour", () => userSource.Configuration.CustomColours["Lookup"] = Color4.Red);
            AddAssert("Check colour lookup", () => requester.GetConfig<SkinCustomColourLookup, Color4>(new SkinCustomColourLookup("Lookup"))?.Value == Color4.Red);
        }

        [Test]
        public void TestGlobalLookup()
        {
            AddAssert("Check combo colours", () => requester.GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value?.Count > 0);
        }

        [Test]
        public void TestWrongColourType()
        {
            AddStep("Add config colour", () => userSource.Configuration.CustomColours["Lookup"] = Color4.Red);

            AddAssert("perform incorrect lookup", () =>
            {
                try
                {
                    requester.GetConfig<SkinCustomColourLookup, int>(new SkinCustomColourLookup("Lookup"));
                    return false;
                }
                catch
                {
                    return true;
                }
            });
        }

        [Test]
        public void TestEmptyComboColours()
        {
            AddAssert("Check retrieved combo colours is skin default colours", () =>
                requester.GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value?.SequenceEqual(SkinConfiguration.DefaultComboColours) ?? false);
        }

        [Test]
        public void TestEmptyComboColoursNoFallback()
        {
            AddStep("Add custom combo colours to user skin", () => userSource.Configuration.CustomComboColours = new List<Color4>
            {
                new Color4(100, 150, 200, 255),
                new Color4(55, 110, 166, 255),
                new Color4(75, 125, 175, 255)
            });

            AddStep("Disallow default colours fallback in beatmap skin", () => beatmapSource.Configuration.AllowDefaultComboColoursFallback = false);

            AddAssert("Check retrieved combo colours from user skin", () =>
                userSource.Configuration.ComboColours != null &&
                (requester.GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value?.SequenceEqual(userSource.Configuration.ComboColours) ?? false));
        }

        [Test]
        public void TestNullBeatmapVersionFallsBackToUserSkin()
        {
            AddStep("Set user skin version 2.3", () => userSource.Configuration.LegacyVersion = 2.3m);
            AddStep("Set beatmap skin version null", () => beatmapSource.Configuration.LegacyVersion = null);
            AddAssert("Check legacy version lookup", () => requester.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value == 2.3m);
        }

        [Test]
        public void TestSetBeatmapVersionFallsBackToUserSkin()
        {
            // completely ignoring beatmap versions for simplicity.
            AddStep("Set user skin version 2.3", () => userSource.Configuration.LegacyVersion = 2.3m);
            AddStep("Set beatmap skin version null", () => beatmapSource.Configuration.LegacyVersion = 1.7m);
            AddAssert("Check legacy version lookup", () => requester.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value == 2.3m);
        }

        [Test]
        public void TestNullBeatmapAndUserVersionFallsBackToLatest()
        {
            AddStep("Set user skin version 2.3", () => userSource.Configuration.LegacyVersion = null);
            AddStep("Set beatmap skin version null", () => beatmapSource.Configuration.LegacyVersion = null);
            AddAssert("Check legacy version lookup",
                () => requester.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value == SkinConfiguration.LATEST_VERSION);
        }

        [Test]
        public void TestIniWithNoVersionFallsBackTo1()
        {
            AddStep("Parse skin with no version", () => userSource.Configuration = new LegacySkinDecoder().Decode(new LineBufferedReader(new MemoryStream())));
            AddAssert("Check legacy version lookup", () => requester.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value == 1.0m);
        }

        public enum LookupType
        {
            Test
        }

        public enum ValueType
        {
            Test1,
            Test2,
            Test3
        }

        public class UserSkinSource : LegacySkin
        {
            public UserSkinSource()
                : base(new SkinInfo(), null, null, string.Empty)
            {
            }
        }

        public class BeatmapSkinSource : LegacyBeatmapSkin
        {
            public BeatmapSkinSource()
                : base(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo, null)
            {
            }
        }

        public partial class SkinRequester : Drawable, ISkin
        {
            private ISkinSource skin;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                this.skin = skin;
            }

            public Drawable GetDrawableComponent(ISkinComponentLookup lookup) => skin.GetDrawableComponent(lookup);

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => skin.GetTexture(componentName, wrapModeS, wrapModeT);

            public ISample GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => skin.FindProvider(lookupFunction);
        }
    }
}
