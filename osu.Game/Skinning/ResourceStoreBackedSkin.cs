// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An <see cref="ISkin"/> that uses an underlying <see cref="IResourceStore{T}"/> with namespaces for resources retrieval.
    /// </summary>
    public class ResourceStoreBackedSkin : ISkin, IDisposable
    {
        private readonly TextureStore textures;
        private readonly ISampleStore samples;

        public ResourceStoreBackedSkin(IResourceStore<byte[]> resources, GameHost host, AudioManager audio)
        {
            textures = new TextureStore(host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(resources, @"Textures")));
            samples = audio.GetSampleStore(new NamespacedResourceStore<byte[]>(resources, @"Samples"));
        }

        public Drawable? GetDrawableComponent(ISkinComponent component) => null;

        public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => textures.Get(componentName, wrapModeS, wrapModeT);

        public ISample? GetSample(ISampleInfo sampleInfo)
        {
            foreach (string? lookup in sampleInfo.LookupNames)
            {
                ISample? sample = samples.Get(lookup);
                if (sample != null)
                    return sample;
            }

            return null;
        }

        public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup) => null;

        public void Dispose()
        {
            textures.Dispose();
            samples.Dispose();
        }
    }
}
