// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Drawables
{
    public static class DrawablesExtensions
    {
        /// <summary>
        /// Adjusts <see cref="Drawable.Blending"/> after a delay.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformBlendingMode<T>(this T drawable, BlendingParameters newValue, double delay = 0)
            where T : Drawable
            => drawable.TransformTo(drawable.PopulateTransform(new TransformBlendingParameters(), newValue, delay));
    }

    public class TransformBlendingParameters : Transform<BlendingParameters, Drawable>
    {
        private BlendingParameters valueAt(double time)
            => time < EndTime ? StartValue : EndValue;

        public override string TargetMember => nameof(Drawable.Blending);

        protected override void Apply(Drawable d, double time) => d.Blending = valueAt(time);
        protected override void ReadIntoStartValue(Drawable d) => StartValue = d.Blending;
    }
}
