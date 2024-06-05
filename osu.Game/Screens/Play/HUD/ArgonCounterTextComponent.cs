// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Text;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonCounterTextComponent : CompositeDrawable, IHasText
    {
        private readonly ArgonCounterSpriteText wireframesPart;
        private readonly ArgonCounterSpriteText textPart;
        private readonly OsuSpriteText labelText;

        public IBindable<float> WireframeOpacity { get; } = new BindableFloat();
        public Bindable<bool> ShowLabel { get; } = new BindableBool();

        public Container NumberContainer { get; private set; }

        public LocalisableString Text
        {
            get => textPart.Text;
            set => textPart.Text = value;
        }

        /// <summary>
        /// The template for the wireframe displayed behind the <see cref="Text"/>.
        /// Any character other than a dot is interpreted to mean a full segmented display "wireframe".
        /// </summary>
        public string WireframeTemplate
        {
            get => wireframeTemplate;
            set => wireframesPart.Text = wireframeTemplate = value;
        }

        private string wireframeTemplate = string.Empty;

        public ArgonCounterTextComponent(Anchor anchor, LocalisableString? label = null)
        {
            Anchor = anchor;
            Origin = anchor;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                labelText = new OsuSpriteText
                {
                    Alpha = 0,
                    Text = label.GetValueOrDefault(),
                    Font = OsuFont.Torus.With(size: 12, weight: FontWeight.Bold),
                    Margin = new MarginPadding { Left = 2.5f },
                },
                NumberContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        wireframesPart = new ArgonCounterSpriteText(wireframesLookup)
                        {
                            Anchor = anchor,
                            Origin = anchor,
                        },
                        textPart = new ArgonCounterSpriteText(textLookup)
                        {
                            Anchor = anchor,
                            Origin = anchor,
                        },
                    }
                }
            };
        }

        private string textLookup(char c)
        {
            switch (c)
            {
                case '.':
                    return @"dot";

                case '%':
                    return @"percentage";

                default:
                    return c.ToString();
            }
        }

        private string wireframesLookup(char c)
        {
            if (c == '.') return @"dot";

            return @"wireframes";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            labelText.Colour = colours.Blue0;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            WireframeOpacity.BindValueChanged(v => wireframesPart.Alpha = v.NewValue, true);
            ShowLabel.BindValueChanged(s =>
            {
                labelText.Alpha = s.NewValue ? 1 : 0;
                NumberContainer.Y = s.NewValue ? 12 : 0;
            }, true);
        }

        private partial class ArgonCounterSpriteText : OsuSpriteText
        {
            private readonly Func<char, string> getLookup;

            private GlyphStore glyphStore = null!;

            protected override char FixedWidthReferenceCharacter => '5';

            public ArgonCounterSpriteText(Func<char, string> getLookup)
            {
                this.getLookup = getLookup;

                Shadow = false;
                UseFullGlyphHeight = false;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                const string font_name = @"argon-counter";

                Spacing = new Vector2(-2f, 0f);
                Font = new FontUsage(font_name, 1);
                glyphStore = new GlyphStore(font_name, textures, getLookup);

                // cache common lookups ahead of time.
                foreach (char c in new[] { '.', '%', 'x' })
                    glyphStore.Get(font_name, c);
                for (int i = 0; i < 10; i++)
                    glyphStore.Get(font_name, (char)('0' + i));
            }

            protected override TextBuilder CreateTextBuilder(ITexturedGlyphLookupStore store) => base.CreateTextBuilder(glyphStore);

            private class GlyphStore : ITexturedGlyphLookupStore
            {
                private readonly string fontName;
                private readonly TextureStore textures;
                private readonly Func<char, string> getLookup;

                private readonly Dictionary<char, ITexturedCharacterGlyph?> cache = new Dictionary<char, ITexturedCharacterGlyph?>();

                public GlyphStore(string fontName, TextureStore textures, Func<char, string> getLookup)
                {
                    this.fontName = fontName;
                    this.textures = textures;
                    this.getLookup = getLookup;
                }

                public ITexturedCharacterGlyph? Get(string? fontName, char character)
                {
                    // We only service one font.
                    if (fontName != this.fontName)
                        return null;

                    if (cache.TryGetValue(character, out var cached))
                        return cached;

                    string lookup = getLookup(character);
                    var texture = textures.Get($"Gameplay/Fonts/{fontName}-{lookup}");

                    TexturedCharacterGlyph? glyph = null;

                    if (texture != null)
                        glyph = new TexturedCharacterGlyph(new CharacterGlyph(character, 0, 0, texture.Width, texture.Height, null), texture, 0.125f);

                    cache[character] = glyph;
                    return glyph;
                }

                public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));
            }
        }
    }
}
