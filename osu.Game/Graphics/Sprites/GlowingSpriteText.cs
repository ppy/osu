// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Sprites
{
    public class GlowingSpriteText : Container
    {
        private readonly OsuSpriteText spriteText, blurredText;

        private string text = string.Empty;

        public string Text
        {
            get => text;
            set
            {
                text = value;

                spriteText.Text = text;
                glowingText.Text = text;
            }
        }

        private FontUsage font = OsuFont.Default.With(fixedWidth: true);

        public FontUsage Font
        {
            get => font;
            set
            {
                font = value.With(fixedWidth: true);

                spriteText.Font = font;
                glowingText.Font = font;
            }
        }

        private Vector2 textSize;

        public Vector2 TextSize
        {
            get => textSize;
            set
            {
                textSize = value;

                spriteText.Size = textSize;
                glowingText.Size = textSize;
            }
        }

        public ColourInfo TextColour
        {
            get => spriteText.Colour;
            set => spriteText.Colour = value;
        }

        public ColourInfo GlowColour
        {
            get => glowingText.Colour;
            set => glowingText.Colour = value;
        }

        public GlowingSpriteText()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new BufferedContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BlurSigma = new Vector2(4),
                    CacheDrawnFrameBuffer = true,
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingMode.Additive,
                    Size = new Vector2(3f),
                    Children = new[]
                    {
                        blurredText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = font,
                            Text = text,
                            Shadow = false,
                        },
                    },
                },
                spriteText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = font,
                    Text = text,
                    Shadow = false,
                },
            };
        }
    }
}
