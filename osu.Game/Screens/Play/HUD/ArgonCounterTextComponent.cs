// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Text;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonCounterTextComponent : CompositeDrawable, IHasText
    {
        private readonly LocalisableString? label;

        private readonly ArgonCounterSpriteText wireframesPart;
        private readonly ArgonCounterSpriteText textPart;

        public IBindable<float> WireframeOpacity { get; } = new BindableFloat();

        public LocalisableString Text
        {
            get => textPart.Text;
            set
            {
                wireframesPart.Text = FormatWireframes(value);
                textPart.Text = value;
            }
        }

        public ArgonCounterTextComponent(Anchor anchor, LocalisableString? label = null, Vector2? spacing = null)
        {
            Anchor = anchor;
            Origin = anchor;

            this.label = label;

            wireframesPart = new ArgonCounterSpriteText(c =>
            {
                if (c == '.')
                    return @"dot";

                return @"wireframes";
            })
            {
                Anchor = anchor,
                Origin = anchor,
                Spacing = spacing ?? new Vector2(-2, 0),
            };
            textPart = new ArgonCounterSpriteText(c =>
            {
                if (c == '.')
                    return @"dot";

                if (c == '%')
                    return @"percentage";

                return c.ToString();
            })
            {
                Anchor = anchor,
                Origin = anchor,
                Spacing = spacing ?? new Vector2(-2, 0),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Alpha = label != null ? 1 : 0,
                        Text = label.GetValueOrDefault(),
                        Font = OsuFont.Torus.With(size: 12, weight: FontWeight.Bold),
                        Colour = colours.Blue0,
                        Margin = new MarginPadding { Left = 2.5f },
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            wireframesPart,
                            textPart,
                        }
                    }
                }
            };
        }

        protected virtual LocalisableString FormatWireframes(LocalisableString text) => text;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            WireframeOpacity.BindValueChanged(v => wireframesPart.Alpha = v.NewValue, true);
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
            private void load(ISkinSource skin)
            {
                // todo: rename font
                Font = new FontUsage(@"argon-score", 1);
                glyphStore = new GlyphStore(skin, getLookup);
            }

            protected override TextBuilder CreateTextBuilder(ITexturedGlyphLookupStore store) => base.CreateTextBuilder(glyphStore);

            private class GlyphStore : ITexturedGlyphLookupStore
            {
                private readonly ISkin skin;
                private readonly Func<char, string> getLookup;

                public GlyphStore(ISkin skin, Func<char, string> getLookup)
                {
                    this.skin = skin;
                    this.getLookup = getLookup;
                }

                public ITexturedCharacterGlyph? Get(string fontName, char character)
                {
                    string lookup = getLookup(character);
                    var texture = skin.GetTexture($"{fontName}-{lookup}");

                    if (texture == null)
                        return null;

                    return new TexturedCharacterGlyph(new CharacterGlyph(character, 0, 0, texture.Width, texture.Height, null), texture, 0.125f);
                }

                public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));
            }
        }
    }
}
