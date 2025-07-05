// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Graphics.Textures;
using osu.Framework.Text;

namespace osu.Game.Graphics
{
    public class EmojiStore : ITextureStore, ITexturedGlyphLookupStore
    {
        public const string FONT_NAME = @"Emoji";

        private readonly TextureStore textures;

        public EmojiStore(TextureStore textures)
        {
            this.textures = textures;
        }

        public ITexturedCharacterGlyph? Get(string? fontName, Grapheme character)
        {
            if (string.IsNullOrEmpty(fontName) || fontName == FONT_NAME)
            {
                var texture = textures.Get($@"{FONT_NAME}/{string.Join(
                    "_",
                    character.ToString().EnumerateRunes().Select(rune => rune.Value.ToString("X").ToLower(CultureInfo.InvariantCulture)))}");
                return texture != null ? new EmojiGlyph(texture, character) : null;
            }

            return null;
        }

        public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, Grapheme character) => Task.Run(() => Get(fontName, character));

        public Texture? Get(string name, WrapMode wrapModeS, WrapMode wrapModeT) => null;

        public Texture Get(string name) => throw new NotImplementedException();

        public Task<Texture> GetAsync(string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Stream GetStream(string name) => throw new NotImplementedException();

        public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

        public Task<Texture?> GetAsync(string name, WrapMode wrapModeS, WrapMode wrapModeT, CancellationToken cancellationToken = default) => throw new NotImplementedException();

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

        public void Dispose()
        {
            textures.Dispose();
        }
    }
}
