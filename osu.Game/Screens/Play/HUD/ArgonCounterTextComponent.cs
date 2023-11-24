// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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
        public Bindable<int> RequiredDisplayDigits { get; } = new BindableInt();
        public Bindable<bool> ShowLabel { get; } = new BindableBool();

        public Container NumberContainer { get; private set; }

        public LocalisableString Text
        {
            get => textPart.Text;
            set
            {
                int remainingCount = RequiredDisplayDigits.Value - value.ToString().Count(char.IsDigit);
                string remainingText = remainingCount > 0 ? new string('#', remainingCount) : string.Empty;

                wireframesPart.Text = remainingText + value;
                textPart.Text = value;
            }
        }

        public ArgonCounterTextComponent(Anchor anchor, LocalisableString? label = null)
        {
            Anchor = anchor;
            Origin = anchor;
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
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
            ShowLabel.BindValueChanged(s => labelText.Alpha = s.NewValue ? 1 : 0, true);
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
                Spacing = new Vector2(-2f, 0f);
                Font = new FontUsage(@"argon-counter", 1);
                glyphStore = new GlyphStore(textures, getLookup);
            }

            protected override TextBuilder CreateTextBuilder(ITexturedGlyphLookupStore store) => base.CreateTextBuilder(glyphStore);

            private class GlyphStore : ITexturedGlyphLookupStore
            {
                private readonly TextureStore textures;
                private readonly Func<char, string> getLookup;

                public GlyphStore(TextureStore textures, Func<char, string> getLookup)
                {
                    this.textures = textures;
                    this.getLookup = getLookup;
                }

                public ITexturedCharacterGlyph? Get(string? fontName, char character)
                {
                    string lookup = getLookup(character);
                    var texture = textures.Get($"Gameplay/Fonts/{fontName}-{lookup}");

                    if (texture == null)
                        return null;

                    return new TexturedCharacterGlyph(new CharacterGlyph(character, 0, 0, texture.Width, texture.Height, null), texture, 0.125f);
                }

                public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));
            }
        }
    }
}
