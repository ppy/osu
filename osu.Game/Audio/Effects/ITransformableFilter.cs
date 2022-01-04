// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Audio.Effects
{
    public interface ITransformableFilter
    {
        /// <summary>
        /// The filter cutoff.
        /// </summary>
        int Cutoff { get; set; }
    }

    public static class FilterableAudioComponentExtensions
    {
        /// <summary>
        /// Smoothly adjusts filter cutoff over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> CutoffTo<T>(this T component, int newCutoff, double duration = 0, Easing easing = Easing.None)
            where T : class, ITransformableFilter, IDrawable
            => component.CutoffTo(newCutoff, duration, new DefaultEasingFunction(easing));

        /// <summary>
        /// Smoothly adjusts filter cutoff over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> CutoffTo<T>(this TransformSequence<T> sequence, int newCutoff, double duration = 0, Easing easing = Easing.None)
            where T : class, ITransformableFilter, IDrawable
            => sequence.CutoffTo(newCutoff, duration, new DefaultEasingFunction(easing));

        /// <summary>
        /// Smoothly adjusts filter cutoff over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> CutoffTo<T, TEasing>(this T component, int newCutoff, double duration, TEasing easing)
            where T : class, ITransformableFilter, IDrawable
            where TEasing : IEasingFunction
            => component.TransformTo(nameof(component.Cutoff), newCutoff, duration, easing);

        /// <summary>
        /// Smoothly adjusts filter cutoff over time.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> CutoffTo<T, TEasing>(this TransformSequence<T> sequence, int newCutoff, double duration, TEasing easing)
            where T : class, ITransformableFilter, IDrawable
            where TEasing : IEasingFunction
            => sequence.Append(o => o.TransformTo(nameof(o.Cutoff), newCutoff, duration, easing));
    }
}
