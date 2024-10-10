// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace osu.Game.Skinning
{
    public class MaxDimensionLimitedTextureLoaderStore : IResourceStore<TextureUpload>
    {
        private readonly IResourceStore<TextureUpload>? textureStore;

        public MaxDimensionLimitedTextureLoaderStore(IResourceStore<TextureUpload>? textureStore)
        {
            this.textureStore = textureStore;
        }

        public void Dispose()
        {
            textureStore?.Dispose();
        }

        public TextureUpload Get(string name)
        {
            var textureUpload = textureStore?.Get(name);

            // NRT not enabled on framework side classes (IResourceStore / TextureLoaderStore), welp.
            if (textureUpload == null)
                return null!;

            return limitTextureUploadSize(textureUpload);
        }

        public async Task<TextureUpload> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            // NRT not enabled on framework side classes (IResourceStore / TextureLoaderStore), welp.
            if (textureStore == null)
                return null!;

            var textureUpload = await textureStore.GetAsync(name, cancellationToken).ConfigureAwait(false);

            if (textureUpload == null)
                return null!;

            return await Task.Run(() => limitTextureUploadSize(textureUpload), cancellationToken).ConfigureAwait(false);
        }

        private TextureUpload limitTextureUploadSize(TextureUpload textureUpload)
        {
            // So there's a thing where some users have taken it upon themselves to create skin elements of insane dimensions.
            // To the point where GPUs cannot load the textures (along with most image editor apps).
            // To work around this, let's look out for any stupid images and shrink them down into a usable size.
            const int max_supported_texture_size = 8192;

            if (textureUpload.Height > max_supported_texture_size || textureUpload.Width > max_supported_texture_size)
            {
                var image = Image.LoadPixelData(textureUpload.Data, textureUpload.Width, textureUpload.Height);

                // The original texture upload will no longer be returned or used.
                textureUpload.Dispose();

                image.Mutate(i => i.Resize(new Size(
                    Math.Min(textureUpload.Width, max_supported_texture_size),
                    Math.Min(textureUpload.Height, max_supported_texture_size)
                )));

                return new TextureUpload(image);
            }

            return textureUpload;
        }

        public Stream? GetStream(string name) => textureStore?.GetStream(name);

        public IEnumerable<string> GetAvailableResources() => textureStore?.GetAvailableResources() ?? Array.Empty<string>();
    }
}
