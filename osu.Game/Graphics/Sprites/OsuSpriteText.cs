// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Graphics.Sprites
{
    public class OsuSpriteText : SpriteText
    {
        public OsuSpriteText()
        {
            Shadow = true;
            Font = OsuFont.Default;
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
