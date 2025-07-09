// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Text;
using osu.Game.IO.Archives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Game.Graphics
{
    public class EmojiStore : TextureStore, ITexturedGlyphLookupStore
    {
        public const string FONT_NAME = @"Emoji";

        public EmojiStore(IRenderer renderer, ResourceStore<byte[]> resourceStore)
            : base(renderer, new EmojiTextureLoaderStore(resourceStore))
        {
        }

        public ITexturedCharacterGlyph? Get(string? fontName, Grapheme character)
        {
            if (string.IsNullOrEmpty(fontName) || fontName == FONT_NAME)
            {
                // Convert the character to a sequence of hex values joined by underscores
                // Example: "👍" -> "1f44d"
                // Example: "👨‍👩‍👧‍👦" -> "1f468_200d_1f469_200d_1f467_200d_1f466"
                var texture = Get(string.Join(
                    "_",
                    character.ToString().EnumerateRunes().Select(rune => rune.Value.ToString("X").ToLower(CultureInfo.InvariantCulture))
                ) + ".raw");

                return texture != null ? new EmojiGlyph(texture, character) : null;
            }

            return null;
        }

        public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, Grapheme character) => Task.Run(() => Get(fontName, character));

        private sealed class EmojiTextureLoaderStore : IResourceStore<TextureUpload>
        {
            private readonly ZipArchiveReader store;

            public EmojiTextureLoaderStore(ResourceStore<byte[]> resourceStore)
            {
                store = new ZipArchiveReader(resourceStore.GetStream(@"Textures/Emoji/Emoji.zip"));
            }

            public Task<TextureUpload> GetAsync(string name, CancellationToken cancellationToken = default) =>
                Task.Run(() => Get(name), cancellationToken);

            public TextureUpload Get(string name)
            {
                var stream = store.GetStream(name);

                // Each emoji is stored as a 50x50 pixel RGBA32 image (4 bytes per pixel)
                // Total size = 50 * 50 * 4 = 10000 bytes
                if (stream != null && stream.Length == 50 * 50 * 4)
                {
                    var memory = MemoryAllocator.Default.Allocate<byte>(50 * 50 * 4);

                    stream.ReadExactly(memory.Memory.Span);

                    return new TextureUpload(Image.WrapMemory<Rgba32>(memory, 50, 50));
                }

                return null!;
            }

            public Stream GetStream(string name) => store.GetStream(name);

            public IEnumerable<string> GetAvailableResources() => store.GetAvailableResources();

            #region IDisposable Support

            public void Dispose()
            {
                store.Dispose();
            }

            #endregion
        }

        public class EmojiGlyph : ITexturedCharacterGlyph
        {
            public float XOffset => default;
            public float YOffset => default;
            public float XAdvance => 1;
            public float Baseline => 0.79f;
            public Grapheme Character { get; }

            public float GetKerning<T>(T lastGlyph) where T : ICharacterGlyph => 0;

            public Texture Texture { get; }
            public float Width => 1;
            public float Height => 1;
            public bool Coloured => true;

            public EmojiGlyph(Texture texture, Grapheme character)
            {
                Texture = texture;
                Character = character;
            }
        }
    }
}
