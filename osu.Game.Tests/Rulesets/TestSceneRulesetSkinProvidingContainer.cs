// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Tests.Testing;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Rulesets
{
    [HeadlessTest]
    public partial class TestSceneRulesetSkinProvidingContainer : OsuTestScene
    {
        private SkinRequester requester;

        protected override Ruleset CreateRuleset() => new TestSceneRulesetDependencies.TestRuleset();

        [Test]
        public void TestRulesetResources()
        {
            setupProviderStep();

            AddAssert("ruleset texture retrieved via skin", () => requester.GetTexture("test-image") != null);
            AddAssert("ruleset sample retrieved via skin", () => requester.GetSample(new SampleInfo("test-sample")) != null);
        }

        [Test]
        public void TestEarlyAddedSkinRequester()
        {
            Texture textureOnLoad = null;

            AddStep("setup provider", () =>
            {
                requester = new SkinRequester();
                requester.OnLoadAsync += () => textureOnLoad = requester.GetTexture("test-image");

                Child = new RulesetSkinProvidingContainer(Ruleset.Value.CreateInstance(), Beatmap.Value.Beatmap, Beatmap.Value.Skin)
                {
                    Child = requester
                };
            });

            AddAssert("requester got correct initial texture", () => textureOnLoad != null);
        }

        private void setupProviderStep()
        {
            AddStep("setup provider", () =>
            {
                Child = new RulesetSkinProvidingContainer(Ruleset.Value.CreateInstance(), Beatmap.Value.Beatmap, Beatmap.Value.Skin)
                    .WithChild(requester = new SkinRequester());
            });
        }

        private partial class SkinRequester : Drawable, ISkin
        {
            private ISkinSource skin;

            public event Action OnLoadAsync;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                this.skin = skin;

                OnLoadAsync?.Invoke();
            }

            public Drawable GetDrawableComponent(ISkinComponentLookup lookup) => skin.GetDrawableComponent(lookup);

            public Texture GetTexture(string componentName, WrapMode wrapModeS = default, WrapMode wrapModeT = default) => skin.GetTexture(componentName);

            public ISample GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);
        }
    }
}
