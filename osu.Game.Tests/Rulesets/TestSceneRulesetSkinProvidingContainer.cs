// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
 // See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Tests.Testing;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Rulesets
{
    public class TestSceneRulesetSkinProvidingContainer : OsuTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; }

        private SkinRequester requester;

        protected override Ruleset CreateRuleset() => new TestRuleset();

        [Test]
        public void TestEarlyAddedSkinRequester()
        {
            ISample transformerSampleOnBdl = null;

            // need a legacy skin to plug the TestRuleset's legacy transformer, which is required for testing this.
            AddStep("set legacy skin", () => skins.CurrentSkinInfo.Value = DefaultLegacySkin.Info);

            AddStep("setup provider", () =>
            {
                var rulesetSkinProvider = new RulesetSkinProvidingContainer(Ruleset.Value.CreateInstance(), Beatmap.Value.Beatmap, Beatmap.Value.Skin);

                rulesetSkinProvider.Add(requester = new SkinRequester());

                requester.OnBdl += () => transformerSampleOnBdl = requester.GetSample(new SampleInfo(TestLegacySkinTransformer.VIRTUAL_SAMPLE_NAME));

                Child = rulesetSkinProvider;
            });

            AddAssert("requester got correct initial sample", () => transformerSampleOnBdl != null);
        }

        private class SkinRequester : Drawable, ISkin
        {
            private ISkinSource skin;

            public event Action OnBdl;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                this.skin = skin;

                OnBdl?.Invoke();
            }

            public Drawable GetDrawableComponent(ISkinComponent component) => skin.GetDrawableComponent(component);

            public Texture GetTexture(string componentName, WrapMode wrapModeS = default, WrapMode wrapModeT = default) => skin.GetTexture(componentName);

            public ISample GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);
        }

        private class TestRuleset : TestSceneRulesetDependencies.TestRuleset
        {
            public override ISkin CreateLegacySkinProvider(ISkin skin, IBeatmap beatmap) => new TestLegacySkinTransformer(skin);
        }

        private class TestLegacySkinTransformer : LegacySkinTransformer
        {
            public const string VIRTUAL_SAMPLE_NAME = "virtual-test-sample";

            public TestLegacySkinTransformer([NotNull] ISkin skin)
                : base(skin)
            {
            }

            public override ISample GetSample(ISampleInfo sampleInfo)
            {
                if (sampleInfo.LookupNames.Single() == VIRTUAL_SAMPLE_NAME)
                    return new SampleVirtual();

                return base.GetSample(sampleInfo);
            }
        }
    }
}
