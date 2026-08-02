// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;

namespace osu.Game.Graphics.Sprites
{
    public partial class GlowingSpriteText : GlowingDrawable, IHasText
    {
        private const float blur_sigma = 3f;

        private OsuSpriteText text = null!;

        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        public FontUsage Font
        {
            get => text.Font;
            set => text.Font = value.With(fixedWidth: true);
        }

        public Vector2 TextSize
        {
            get => text.Size;
            set => text.Size = value;
        }

        public ColourInfo TextColour
        {
            get => text.Colour;
            set => text.Colour = value;
        }

        public Vector2 Spacing
        {
            get => text.Spacing;
            set => text.Spacing = value;
        }

        public bool UseFullGlyphHeight
        {
            get => text.UseFullGlyphHeight;
            set => text.UseFullGlyphHeight = value;
        }

        public Bindable<string> Current
        {
            get => text.Current;
            set => text.Current = value;
        }

        public GlowingSpriteText()
        {
            BlurSigma = new Vector2(blur_sigma);
            EffectBlending = BlendingParameters.Additive;
        }

        protected override Drawable CreateDrawable() => text = new OsuSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Shadow = false,
        };
    }
}
