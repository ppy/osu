// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.MathUtils;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Graphics.Sprites
{
    public class OsuSpriteText : SpriteText
    {
        public const float FONT_SIZE = 16;

        public OsuSpriteText()
        {
            Shadow = true;
            TextSize = FONT_SIZE;
        }

        public void Truncate(float maxWidth, bool ellipsis = false)
        {
            if (ellipsis)
            {
                Texture period = this.GetTextureForCharacter('.');
                // Scale the width to the current TextSize
                float scaleFactor = (float)period.Width / period.Height;
                maxWidth -= 3 * (period.Width - (period.Height - this.TextSize) * scaleFactor);

            }

            float totalWidth = 0;
            for (int a = 0; a < this.Text.Length; a++)
            {
                Texture t = this.GetTextureForCharacter(this.Text[a]);
                float scaleFactor = (float)t.Width / t.Height;
                totalWidth += t.Width - (t.Height - this.TextSize) * scaleFactor;
                if (totalWidth > maxWidth)
                {
                    this.Text = ellipsis ? this.Text.Substring(0, a - 1) + "..." : this.Text.Substring(0, a - 1);
                    return;
                }
            }
        }

        public float CalculateTextWidth()
        {
            float totalWidth = 0;
            foreach (char c in this.Text)
            {
                Texture t = this.GetTextureForCharacter(c);
                // Scale the width to the current TextSize
                totalWidth += this.TextSize / t.Height * t.Width;

            }
            return totalWidth;
        }

        protected override Drawable CreateFallbackCharacterDrawable()
        {
            var tex = GetTextureForCharacter('?');

            if (tex != null)
            {
                float adjust = (RNG.NextSingle() - 0.5f) * 2;
                return new Sprite
                {
                    Texture = tex,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Scale = new Vector2(1 + adjust * 0.2f),
                    Rotation = adjust * 15,
                    Colour = Color4.White,
                };
            }

            return base.CreateFallbackCharacterDrawable();
        }
    }

    public static class OsuSpriteTextTransformExtensions
    {
        /// <summary>
        /// Sets <see cref="OsuSpriteText.Text"/> to a new value after a duration.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformTextTo<T>(this T spriteText, string newText, double duration = 0, Easing easing = Easing.None)
            where T : OsuSpriteText
            => spriteText.TransformTo(nameof(OsuSpriteText.Text), newText, duration, easing);

        /// <summary>
        /// Sets <see cref="OsuSpriteText.Text"/> to a new value after a duration.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformTextTo<T>(this TransformSequence<T> t, string newText, double duration = 0, Easing easing = Easing.None)
            where T : OsuSpriteText
            => t.Append(o => o.TransformTextTo(newText, duration, easing));
    }
}
