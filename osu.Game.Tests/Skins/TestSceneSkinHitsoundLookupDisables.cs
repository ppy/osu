// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
    public partial class TestSceneSkinHitsoundLookupDisables : OsuTestScene
    {
        private UserSkinSource userSource;
        private SkinRequester requester;

        [Resolved]
        private OsuConfigManager config { get; set; }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            var beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo);

            Add(new SkinProvidingContainer(userSource = new UserSkinSource())
                .WithChild(new RulesetSkinProvidingContainer(new OsuRuleset(), beatmap, null)
                    .WithChild(requester = new SkinRequester())));
        });

        [TestCase(false)]
        [TestCase(true)]
        public void TestHitsoundLookup(bool ignoreSkinHitsounds)
        {
            AddStep($"Set ignore skin hitsounds to {ignoreSkinHitsounds}", () => config.SetValue(OsuSetting.IgnoreSkinHitsounds, ignoreSkinHitsounds));

            // When IgnoreSkinHitsounds is true, the custom skin hitsound should be ignored
            // and we should get null (falling back to default skin)
            bool expectNull = ignoreSkinHitsounds;

            AddAssert($"Check hitsound lookup is {(expectNull ? "null" : "from user skin")}", 
                () => (requester.GetSample(new HitSampleInfo("hitnormal")) == null) == expectNull);
        }

        public class UserSkinSource : LegacySkin
        {
            public UserSkinSource()
                : base(new SkinInfo(), null, null, string.Empty)
            {
            }

            public override ISample GetSample(ISampleInfo sampleInfo)
            {
                // Return a mock sample for hitsounds
                if (sampleInfo is HitSampleInfo)
                    return new SampleVirtual("user-hitsound");

                return base.GetSample(sampleInfo);
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

            public ISkin FindProvider(System.Func<ISkin, bool> lookupFunction) => skin.FindProvider(lookupFunction);
        }
    }
}
