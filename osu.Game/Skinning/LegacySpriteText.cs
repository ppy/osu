// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Text;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Skinning
{
    public sealed partial class LegacySpriteText : OsuSpriteText
    {
        public Vector2? MaxSizePerGlyph { get; init; }
        public bool FixedWidth { get; init; }

        private readonly LegacyFont font;

        private LegacyGlyphStore glyphStore = null!;

        protected override char FixedWidthReferenceCharacter => '5';

        protected override char[] FixedWidthExcludeCharacters => new[] { ',', '.', '%', 'x' };

        // ReSharper disable once UnusedMember.Global
        // being unused is the point here
        public new FontUsage Font
        {
            get => base.Font;
            set => throw new InvalidOperationException(@"Attempting to use this setter will not work correctly. "
                                                       + $@"Use specific init-only properties exposed by {nameof(LegacySpriteText)} instead.");
        }

        public LegacySpriteText(LegacyFont font)
        {
            this.font = font;

            Shadow = false;
            UseFullGlyphHeight = false;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            string fontPrefix = skin.GetFontPrefix(font);
            base.Font = new FontUsage(fontPrefix, 1, fixedWidth: FixedWidth);
            Spacing = new Vector2(-skin.GetFontOverlap(font), 0);

            glyphStore = new LegacyGlyphStore(fontPrefix, skin, MaxSizePerGlyph);
        }

        protected override TextBuilder CreateTextBuilder(ITexturedGlyphLookupStore store) => base.CreateTextBuilder(glyphStore);

        private class LegacyGlyphStore : ITexturedGlyphLookupStore
        {
            private readonly ISkin skin;
            private readonly Vector2? maxSize;

            private readonly string fontName;

            private readonly Dictionary<char, ITexturedCharacterGlyph?> cache = new Dictionary<char, ITexturedCharacterGlyph?>();

            public LegacyGlyphStore(string fontName, ISkin skin, Vector2? maxSize)
            {
                this.fontName = fontName;
                this.skin = skin;
                this.maxSize = maxSize;
            }

            public ITexturedCharacterGlyph? Get(string? fontName, char character)
            {
                // We only service one font.
                if (fontName != this.fontName)
                    return null;

                if (cache.TryGetValue(character, out var cached))
                    return cached;

                string lookup = getLookupName(character);

                var texture = skin.GetTexture($"{fontName}-{lookup}");

                TexturedCharacterGlyph? glyph = null;

                if (texture != null)
                {
                    if (maxSize != null)
                        texture = texture.WithMaximumSize(maxSize.Value);

                    glyph = new TexturedCharacterGlyph(new CharacterGlyph(character, 0, 0, texture.Width, texture.Height, null), texture, 1f / texture.ScaleAdjust);
                }

                cache[character] = glyph;
                return glyph;
            }

            private static string getLookupName(char character)
            {
                switch (character)
                {
                    case ',':
                        return "comma";

                    case '.':
                        return "dot";

                    case '%':
                        return "percent";

                    default:
                        return character.ToString();
                }
            }

            public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));
        }
    }
}
