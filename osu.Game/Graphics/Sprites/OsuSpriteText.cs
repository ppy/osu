// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
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
