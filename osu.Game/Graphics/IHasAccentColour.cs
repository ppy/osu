// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Graphics
{
    /// <summary>
    /// A type of drawable that has an accent colour.
    /// The accent colour is used to colorize various objects inside a drawable
    /// without colorizing the drawable itself.
    /// </summary>
    public interface IHasAccentColour : IDrawable
    {
        Color4 AccentColour { get; set; }
    }

    public static class AccentedColourExtensions
    {
        /// <summary>
        /// Smoothly adjusts <see cref="IHasAccentColour.AccentColour"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeAccent<T>(this T accentedDrawable, Color4 newColour, double duration = 0, Easing easing = Easing.None)
            where T : class, IHasAccentColour
            => accentedDrawable.TransformTo(nameof(accentedDrawable.AccentColour), newColour, duration, easing);

        /// <summary>
        /// Smoothly adjusts <see cref="IHasAccentColour.AccentColour"/> over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> FadeAccent<T>(this TransformSequence<T> t, Color4 newColour, double duration = 0, Easing easing = Easing.None)
            where T : Drawable, IHasAccentColour
            => t.Append(o => o.FadeAccent(newColour, duration, easing));
    }
}
