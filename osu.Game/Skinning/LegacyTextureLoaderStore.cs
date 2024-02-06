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
    public class LegacyTextureLoaderStore : IResourceStore<TextureUpload>
    {
        private readonly IResourceStore<TextureUpload>? wrappedStore;

        public LegacyTextureLoaderStore(IResourceStore<TextureUpload>? wrappedStore)
        {
            this.wrappedStore = wrappedStore;
        }

        public TextureUpload Get(string name)
        {
            var textureUpload = wrappedStore?.Get(name);

            if (textureUpload == null)
                return null!;

            return shouldConvertToGrayscale(name)
                ? convertToGrayscale(textureUpload)
                : textureUpload;
        }

        public Task<TextureUpload> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        {
            var textureUpload = wrappedStore?.Get(name);

            if (textureUpload == null)
                return null!;

            return shouldConvertToGrayscale(name)
                ? Task.Run(() => convertToGrayscale(textureUpload), cancellationToken)
                : Task.FromResult(textureUpload);
        }

        // https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/Graphics/Textures/TextureManager.cs#L91-L96
        private static readonly string[] grayscale_sprites =
        {
            @"taiko-bar-right",
            @"taikobigcircle",
            @"taikohitcircle",
            @"taikohitcircleoverlay"
        };

        private bool shouldConvertToGrayscale(string name)
        {
            foreach (string grayscaleSprite in grayscale_sprites)
            {
                // unfortunately at this level of lookup we can encounter `@2x` scale suffixes in the name,
                // so straight equality cannot be used.
                if (name.StartsWith(grayscaleSprite, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private TextureUpload convertToGrayscale(TextureUpload textureUpload)
        {
            var image = Image.LoadPixelData(textureUpload.Data.ToArray(), textureUpload.Width, textureUpload.Height);

            // stable uses `0.299 * r + 0.587 * g + 0.114 * b`
            // (https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/Graphics/Textures/pTexture.cs#L138-L153)
            // which matches mode BT.601 (https://en.wikipedia.org/wiki/Grayscale#Luma_coding_in_video_systems)
            image.Mutate(i => i.Grayscale(GrayscaleMode.Bt601));

            return new TextureUpload(image);
        }

        public Stream? GetStream(string name) => wrappedStore?.GetStream(name);

        public IEnumerable<string> GetAvailableResources() => wrappedStore?.GetAvailableResources() ?? Array.Empty<string>();

        public void Dispose()
        {
            wrappedStore?.Dispose();
        }
    }
}
