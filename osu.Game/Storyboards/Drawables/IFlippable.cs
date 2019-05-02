// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Drawables
{
    public interface IFlippable : ITransformable
    {
        bool FlipH { get; set; }
        bool FlipV { get; set; }
    }

    public class TransformFlipH : Transform<bool, IFlippable>
    {
        private bool valueAt(double time)
            => time < EndTime ? StartValue : EndValue;

        public override string TargetMember => nameof(IFlippable.FlipH);

        protected override void Apply(IFlippable d, double time) => d.FlipH = valueAt(time);
        protected override void ReadIntoStartValue(IFlippable d) => StartValue = d.FlipH;
    }

    public class TransformFlipV : Transform<bool, IFlippable>
    {
        private bool valueAt(double time)
            => time < EndTime ? StartValue : EndValue;

        public override string TargetMember => nameof(IFlippable.FlipV);

        protected override void Apply(IFlippable d, double time) => d.FlipV = valueAt(time);
        protected override void ReadIntoStartValue(IFlippable d) => StartValue = d.FlipV;
    }

    public static class FlippableExtensions
    {
        /// <summary>
        /// Adjusts <see cref="IFlippable.FlipH"/> after a delay.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformFlipH<T>(this T flippable, bool newValue, double delay = 0)
            where T : IFlippable
            => flippable.TransformTo(flippable.PopulateTransform(new TransformFlipH(), newValue, delay));

        /// <summary>
        /// Adjusts <see cref="IFlippable.FlipV"/> after a delay.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformFlipV<T>(this T flippable, bool newValue, double delay = 0)
            where T : IFlippable
            => flippable.TransformTo(flippable.PopulateTransform(new TransformFlipV(), newValue, delay));
    }
}
