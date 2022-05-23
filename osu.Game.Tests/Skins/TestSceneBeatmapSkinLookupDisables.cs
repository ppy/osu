// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    [HeadlessTest]
    public class TestSceneBeatmapSkinLookupDisables : OsuTestScene
    {
        private UserSkinSource userSource;
        private BeatmapSkinSource beatmapSource;
        private SkinRequester requester;

        [Resolved]
        private OsuConfigManager config { get; set; }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Add(new SkinProvidingContainer(userSource = new UserSkinSource())
                .WithChild(new BeatmapSkinProvidingContainer(beatmapSource = new BeatmapSkinSource())
                    .WithChild(requester = new SkinRequester())));
        });

        [TestCase(false)]
        [TestCase(true)]
        public void TestDrawableLookup(bool allowBeatmapLookups)
        {
            AddStep($"Set beatmap skin enabled to {allowBeatmapLookups}", () => config.SetValue(OsuSetting.BeatmapSkins, allowBeatmapLookups));

            string expected = allowBeatmapLookups ? "beatmap" : "user";

            AddAssert($"Check lookup is from {expected}", () => requester.GetDrawableComponent(new TestSkinComponent())?.Name == expected);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestProviderLookup(bool allowBeatmapLookups)
        {
            AddStep($"Set beatmap skin enabled to {allowBeatmapLookups}", () => config.SetValue(OsuSetting.BeatmapSkins, allowBeatmapLookups));

            ISkin expected() => allowBeatmapLookups ? (ISkin)beatmapSource : userSource;

            AddAssert("Check lookup is from correct source", () => requester.FindProvider(s => s.GetDrawableComponent(new TestSkinComponent()) != null) == expected());
        }

        public class UserSkinSource : LegacySkin
        {
            public UserSkinSource()
                : base(new SkinInfo(), null, null, string.Empty)
            {
            }

            public override Drawable GetDrawableComponent(ISkinComponent component)
            {
                return new Container { Name = "user" };
            }
        }

        public class BeatmapSkinSource : LegacyBeatmapSkin
        {
            public BeatmapSkinSource()
                : base(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo, null)
            {
            }

            public override Drawable GetDrawableComponent(ISkinComponent component)
            {
                return new Container { Name = "beatmap" };
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

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => skin.GetTexture(componentName, wrapModeS, wrapModeT);

            public ISample GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => skin.FindProvider(lookupFunction);
        }

        private class TestSkinComponent : ISkinComponent
        {
            public string LookupName => string.Empty;
        }
    }
}
