// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    public class TestSceneSkinProvidingContainer : OsuTestScene
    {
        /// <summary>
        /// Ensures that the first inserted skin after resetting (via source change)
        /// is always prioritised over others when providing the same resource.
        /// </summary>
        [Test]
        public void TestPriorityPreservation()
        {
            TestSkinProvidingContainer provider = null;
            TestSkin mostPrioritisedSource = null;

            AddStep("setup sources", () =>
            {
                var sources = new List<TestSkin>();
                for (int i = 0; i < 10; i++)
                    sources.Add(new TestSkin());

                mostPrioritisedSource = sources.First();

                Child = provider = new TestSkinProvidingContainer(sources);
            });

            AddAssert("texture provided by expected skin", () =>
            {
                return provider.FindProvider(s => s.GetTexture(TestSkin.TEXTURE_NAME) != null) == mostPrioritisedSource;
            });

            AddStep("trigger source change", () => provider.TriggerSourceChanged());

            AddAssert("texture still provided by expected skin", () =>
            {
                return provider.FindProvider(s => s.GetTexture(TestSkin.TEXTURE_NAME) != null) == mostPrioritisedSource;
            });
        }

        private class TestSkinProvidingContainer : SkinProvidingContainer
        {
            private readonly IEnumerable<ISkin> sources;

            public TestSkinProvidingContainer(IEnumerable<ISkin> sources)
            {
                this.sources = sources;
            }

            public new void TriggerSourceChanged() => base.TriggerSourceChanged();

            protected override void RefreshSources()
            {
                SetSources(sources);
            }
        }

        private class TestSkin : ISkin
        {
            public const string TEXTURE_NAME = "virtual-texture";

            public Drawable GetDrawableComponent(ISkinComponent component) => throw new System.NotImplementedException();

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                if (componentName == TEXTURE_NAME)
                    return Texture.WhitePixel;

                return null;
            }

            public ISample GetSample(ISampleInfo sampleInfo) => throw new System.NotImplementedException();

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => throw new System.NotImplementedException();
        }
    }
}
