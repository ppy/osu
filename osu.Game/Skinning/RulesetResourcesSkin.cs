// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Audio;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An <see cref="ISkin"/> providing the resources of the ruleset for accessibility during lookups.
    /// </summary>
    public class RulesetResourcesSkin : ISkin
    {
        private readonly TextureStore rulesetTextures;
        private readonly ISampleStore rulesetSamples;

        public RulesetResourcesSkin(Ruleset ruleset, GameHost host, AudioManager audio)
        {
            IResourceStore<byte[]> rulesetResources = ruleset.CreateResourceStore();

            rulesetTextures = new TextureStore(host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(rulesetResources, @"Textures")));
            rulesetSamples = audio.GetSampleStore(new NamespacedResourceStore<byte[]>(rulesetResources, @"Samples"));
        }

        public Drawable GetDrawableComponent(ISkinComponent component) => null;

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => rulesetTextures.Get(componentName, wrapModeS, wrapModeT);

        public ISample GetSample(ISampleInfo sampleInfo)
        {
            foreach (var lookup in sampleInfo.LookupNames)
            {
                ISample sample = rulesetSamples.Get(lookup);
                if (sample != null)
                    return sample;
            }

            return null;
        }

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => null;
    }
}
