// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Text;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Skinning
{
    public sealed class LegacySpriteText : OsuSpriteText
    {
        private readonly LegacyFont font;

        private LegacyGlyphStore glyphStore;

        protected override char FixedWidthReferenceCharacter => '5';

        protected override char[] FixedWidthExcludeCharacters => new[] { ',', '.', '%', 'x' };

        public LegacySpriteText(LegacyFont font)
        {
            this.font = font;
            Shadow = false;
            UseFullGlyphHeight = false;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Font = new FontUsage(skin.GetFontPrefix(font), 1, fixedWidth: true);
            Spacing = new Vector2(-skin.GetFontOverlap(font), 0);

            glyphStore = new LegacyGlyphStore(skin);
        }

        protected override TextBuilder CreateTextBuilder(ITexturedGlyphLookupStore store) => base.CreateTextBuilder(glyphStore);

        private class LegacyGlyphStore : ITexturedGlyphLookupStore
        {
            private readonly ISkin skin;

            public LegacyGlyphStore(ISkin skin)
            {
                this.skin = skin;
            }

            public ITexturedCharacterGlyph Get(string fontName, char character)
            {
                var lookup = getLookupName(character);

                var texture = skin.GetTexture($"{fontName}-{lookup}");

                if (texture == null)
                    return null;

                return new TexturedCharacterGlyph(new CharacterGlyph(character, 0, 0, texture.Width, null), texture, 1f / texture.ScaleAdjust);
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

            public Task<ITexturedCharacterGlyph> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));
        }
    }
}
