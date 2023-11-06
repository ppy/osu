// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Text;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonScoreCounter : GameplayScoreCounter, ISerialisableDrawable
    {
        [SettingSource("Wireframe opacity", "Controls the opacity of the wire frames behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.4f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        public bool UsesFixedAnchor { get; set; }

        protected override LocalisableString FormatCount(long count) => count.ToLocalisableString();

        protected override IHasText CreateText() => new ArgonScoreTextComponent
        {
            RequiredDisplayDigits = { BindTarget = RequiredDisplayDigits },
            WireframeOpacity = { BindTarget = WireframeOpacity },
        };

        private partial class ArgonScoreTextComponent : CompositeDrawable, IHasText
        {
            private readonly ArgonScoreSpriteText wireframesPart;
            private readonly ArgonScoreSpriteText textPart;

            public IBindable<int> RequiredDisplayDigits { get; } = new BindableInt();
            public IBindable<float> WireframeOpacity { get; } = new BindableFloat();

            public LocalisableString Text
            {
                get => textPart.Text;
                set
                {
                    wireframesPart.Text = new string('#', Math.Max(value.ToString().Length, RequiredDisplayDigits.Value));
                    textPart.Text = value;
                }
            }

            public ArgonScoreTextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new[]
                {
                    wireframesPart = new ArgonScoreSpriteText(@"wireframes")
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                    textPart = new ArgonScoreSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                WireframeOpacity.BindValueChanged(v => wireframesPart.Alpha = v.NewValue, true);
            }

            private partial class ArgonScoreSpriteText : OsuSpriteText
            {
                private readonly string? glyphLookupOverride;

                private GlyphStore glyphStore = null!;

                protected override char FixedWidthReferenceCharacter => '5';

                public ArgonScoreSpriteText(string? glyphLookupOverride = null)
                {
                    this.glyphLookupOverride = glyphLookupOverride;

                    Shadow = false;
                    UseFullGlyphHeight = false;
                }

                [BackgroundDependencyLoader]
                private void load(ISkinSource skin)
                {
                    Font = new FontUsage(@"argon-score", 1, fixedWidth: true);
                    Spacing = new Vector2(-2, 0);

                    glyphStore = new GlyphStore(skin, glyphLookupOverride);
                }

                protected override TextBuilder CreateTextBuilder(ITexturedGlyphLookupStore store) => base.CreateTextBuilder(glyphStore);

                private class GlyphStore : ITexturedGlyphLookupStore
                {
                    private readonly ISkin skin;
                    private readonly string? glyphLookupOverride;

                    public GlyphStore(ISkin skin, string? glyphLookupOverride)
                    {
                        this.skin = skin;
                        this.glyphLookupOverride = glyphLookupOverride;
                    }

                    public ITexturedCharacterGlyph? Get(string fontName, char character)
                    {
                        string lookup = glyphLookupOverride ?? character.ToString();
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
}
