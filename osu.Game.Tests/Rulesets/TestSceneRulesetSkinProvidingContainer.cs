// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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
        [Resolved]
        private SkinManager skins { get; set; }

        private SkinRequester requester;

        protected override Ruleset CreateRuleset() => new TestSceneRulesetDependencies.TestRuleset();

        [Test]
        public void TestEarlyAddedSkinRequester()
        {
            Texture textureOnLoad = null;

            AddStep("set skin", () => skins.CurrentSkinInfo.Value = DefaultLegacySkin.Info);

            AddStep("setup provider", () =>
            {
                var rulesetSkinProvider = new RulesetSkinProvidingContainer(Ruleset.Value.CreateInstance(), Beatmap.Value.Beatmap, Beatmap.Value.Skin);

                rulesetSkinProvider.Add(requester = new SkinRequester());

                requester.OnLoadAsync += () => textureOnLoad = requester.GetTexture("hitcircle");

                Child = rulesetSkinProvider;
            });

            AddAssert("requester got correct initial texture", () => textureOnLoad != null);
        }

        private class SkinRequester : Drawable, ISkin
        {
            private ISkinSource skin;

            public event Action OnLoadAsync;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                this.skin = skin;

                OnLoadAsync?.Invoke();
            }

            public Drawable GetDrawableComponent(ISkinComponent component) => skin.GetDrawableComponent(component);

            public Texture GetTexture(string componentName, WrapMode wrapModeS = default, WrapMode wrapModeT = default) => skin.GetTexture(componentName);

            public ISample GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);
        }
    }
}
