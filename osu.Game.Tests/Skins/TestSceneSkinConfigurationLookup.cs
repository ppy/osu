// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    public class TestSceneSkinConfigurationLookup : OsuTestScene
    {
        private LegacySkin source1;
        private LegacySkin source2;
        private SkinRequester requester;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Add(new SkinProvidingContainer(source1 = new SkinSource())
                .WithChild(new SkinProvidingContainer(source2 = new SkinSource())
                    .WithChild(requester = new SkinRequester())));
        });

        [Test]
        public void TestBasicLookup()
        {
            AddStep("Add config values", () =>
            {
                source1.Configuration.ConfigDictionary["Lookup"] = "source1";
                source2.Configuration.ConfigDictionary["Lookup"] = "source2";
            });

            AddAssert("Check lookup finds source2", () => requester.GetConfig<string, string>("Lookup")?.Value == "source2");
        }

        [Test]
        public void TestFloatLookup()
        {
            AddStep("Add config values", () => source1.Configuration.ConfigDictionary["FloatTest"] = "1.1");
            AddAssert("Check float parse lookup", () => requester.GetConfig<string, float>("FloatTest")?.Value == 1.1f);
        }

        [Test]
        public void TestBoolLookup()
        {
            AddStep("Add config values", () => source1.Configuration.ConfigDictionary["BoolTest"] = "1");
            AddAssert("Check bool parse lookup", () => requester.GetConfig<string, bool>("BoolTest")?.Value == true);
        }

        [Test]
        public void TestEnumLookup()
        {
            AddStep("Add config values", () => source1.Configuration.ConfigDictionary["Test"] = "Test2");
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
            AddStep("Add config values", () => source1.Configuration.ConfigDictionary["Lookup"] = null);

            AddAssert("Check lookup null", () =>
            {
                var bindable = requester.GetConfig<string, string>("Lookup");
                return bindable != null && bindable.Value == null;
            });
        }

        [Test]
        public void TestColourLookup()
        {
            AddStep("Add config colour", () => source1.Configuration.CustomColours["Lookup"] = Color4.Red);
            AddAssert("Check colour lookup", () => requester.GetConfig<SkinCustomColourLookup, Color4>(new SkinCustomColourLookup("Lookup"))?.Value == Color4.Red);
        }

        [Test]
        public void TestGlobalLookup()
        {
            AddAssert("Check combo colours", () => requester.GetConfig<GlobalSkinConfiguration, List<Color4>>(GlobalSkinConfiguration.ComboColours)?.Value?.Count > 0);
        }

        [Test]
        public void TestWrongColourType()
        {
            AddStep("Add config colour", () => source1.Configuration.CustomColours["Lookup"] = Color4.Red);

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

        public class SkinSource : LegacySkin
        {
            public SkinSource()
                : base(new SkinInfo(), null, null, string.Empty)
            {
            }
        }

        public class SkinRequester : Drawable, ISkin
        {
            private ISkinSource skin;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                this.skin = skin;
            }

            public Drawable GetDrawableComponent(ISkinComponent component) => skin.GetDrawableComponent(component);

            public Texture GetTexture(string componentName) => skin.GetTexture(componentName);

            public SampleChannel GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);
        }
    }
}
