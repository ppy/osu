// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Graphics.Sprites
{
    public partial class GlowingSpriteText : BufferedContainer, IHasText
    {
        private const float blur_sigma = 3f;

        // Inflate draw quad to prevent glow from trimming at the edges.
        // Padding won't suffice since it will affect text position in cases when it's not centered.
        protected override Quad ComputeScreenSpaceDrawQuad() => base.ComputeScreenSpaceDrawQuad().AABBFloat.Inflate(Blur.KernelSize(blur_sigma));

        private readonly OsuSpriteText text;

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

        public ColourInfo GlowColour
        {
            get => EffectColour;
            set
            {
                EffectColour = value;
                BackgroundColour = value.MultiplyAlpha(0f);
            }
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
            : base(cachedFrameBuffer: true)
        {
            AutoSizeAxes = Axes.Both;
            BlurSigma = new Vector2(blur_sigma);
            RedrawOnScale = false;
            DrawOriginal = true;
            EffectBlending = BlendingParameters.Additive;
            EffectPlacement = EffectPlacement.InFront;
            Child = text = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Shadow = false,
            };
        }
    }
}
