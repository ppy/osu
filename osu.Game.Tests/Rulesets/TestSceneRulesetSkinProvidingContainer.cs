// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Tests.Testing;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Rulesets
{
    public class TestSceneRulesetSkinProvidingContainer : OsuTestScene
    {
        private RulesetSkinProvidingContainer rulesetSkinProvider;
        private SkinRequester requester;

        protected override Ruleset CreateRuleset() => new TestSceneRulesetDependencies.TestRuleset();

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = rulesetSkinProvider = new RulesetSkinProvidingContainer(Ruleset.Value.CreateInstance(), Beatmap.Value.Beatmap, Beatmap.Value.Skin)
                .WithChild(requester = new SkinRequester());
        });

        [Test]
        public void TestRulesetResources()
        {
            AddAssert("ruleset texture retrieved via skin", () => requester.GetTexture("test-image") != null);
            AddAssert("ruleset sample retrieved via skin", () => requester.GetSample(new SampleInfo("test-sample")) != null);
        }

        private class SkinRequester : Drawable, ISkin
        {
            private ISkinSource skin;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                this.skin = skin;
            }

            public Drawable GetDrawableComponent(ISkinComponent component) => skin.GetDrawableComponent(component);

            public Texture GetTexture(string componentName, WrapMode wrapModeS = default, WrapMode wrapModeT = default) => skin.GetTexture(componentName);

            public ISample GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);
        }
    }
}
