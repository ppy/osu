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

namespace osu.Game.Beatmaps
{
    // Implementation of this class is based off of `MaxDimensionLimitedTextureLoaderStore`.
    // If issues are found it's worth checking to make sure similar issues exist there.
    public class BeatmapPanelBackgroundTextureLoaderStore : IResourceStore<TextureUpload>
    {
        // These numbers are taken from the draw visualiser size requirements for song select panel textures at extreme aspect ratios.
        private const int max_height = 130;
        private const int max_width = 1280;

        private readonly IResourceStore<TextureUpload>? textureStore;

        public BeatmapPanelBackgroundTextureLoaderStore(IResourceStore<TextureUpload>? textureStore)
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
            var image = Image.LoadPixelData(textureUpload.Data.ToArray(), textureUpload.Width, textureUpload.Height);

            // The original texture upload will no longer be returned or used.
            textureUpload.Dispose();

            Size size = image.Size();

            float aspectRatio = (float)size.Width / size.Height;

            int usableWidth = Math.Min((int)(max_width * aspectRatio), size.Width);
            int usableHeight = Math.Min(max_height, size.Height);

            // Crop the centre region of the background for now.
            Rectangle cropRectangle = new Rectangle(
                (size.Width - usableWidth) / 2,
                (size.Height - usableHeight) / 2,
                usableWidth,
                usableHeight
            );

            image.Mutate(i => i.Crop(cropRectangle));

            return new TextureUpload(image);
        }

        public Stream? GetStream(string name) => textureStore?.GetStream(name);

        public IEnumerable<string> GetAvailableResources() => textureStore?.GetAvailableResources() ?? Array.Empty<string>();
    }
}
